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
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();
            
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
                Guid id = message.Id;
                string type = message.Type;
                string payload = message.Payload;
                
                try
                {
                    var messageType = typeof(Program).Assembly.GetType(type)!;
                    var deserializedMessage = JsonSerializer.Deserialize(payload, messageType)!;

                    // We should introduce retries here to improve reliability.
                    await bus.Publish(deserializedMessage, stoppingToken);

                    await connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, id },
                        transaction: transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message with ID {OutboxMessageId}", id);

                    await connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc, error_message = @Error
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, Error = ex.Message, Id = id },
                        transaction: transaction);
                }
            }

            await transaction.CommitAsync(stoppingToken);

            _logger.LogInformation("Finished processing outbox.");
            
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
