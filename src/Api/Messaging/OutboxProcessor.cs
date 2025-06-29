using System.Text.Json;
using Dapper;
using MassTransit;
using Npgsql;

namespace ContextDrivenDevelopment.Api.Messaging;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IServiceProvider _services;

    public OutboxProcessor(ILogger<OutboxProcessor> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            
            _logger.LogInformation("Processing outbox...");
            
            await using var connection = await dataSource.OpenConnectionAsync(stoppingToken);
            await using var transaction = await connection.BeginTransactionAsync(stoppingToken);
        
            var messages = (await connection.QueryAsync(
                """
                select id as "Id", type as "Type", payload as "Payload"
                from outbox_messages
                where processed_on_utc is null
                order by created_on_utc limit 100
                """,
                transaction: transaction)).ToList();
        
            _logger.LogInformation("Found {MessageCount} messages to process.", messages.Count);
            
            foreach (var message in messages)
            {
                try
                {
                    var messageType = typeof(Program).Assembly.GetType(message.Type);
                    var deserializedMessage = JsonSerializer.Deserialize(message.Payload, messageType);

                    // We should introduce retries here to improve reliability.
                    await publishEndpoint.Publish(deserializedMessage);

                    await connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, message.Id },
                        transaction: transaction);
                }
                catch (Exception ex)
                {
                    string messageId = message.Id.ToString();
                    _logger.LogError(ex, "Failed to process message with ID {OutboxMessageId}", messageId);

                    await connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc, error_message = @Error
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, Error = ex.Message, message.Id },
                        transaction: transaction);
                }
            }

            await transaction.CommitAsync(stoppingToken);

            _logger.LogInformation("Finished processing outbox.");
            
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
