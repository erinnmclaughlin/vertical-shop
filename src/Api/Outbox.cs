using System.Diagnostics.CodeAnalysis;
using MassTransit;
using Microsoft.Extensions.Options;

namespace VerticalShop.Api;

/// <summary>
/// Configuration options for outbox processing.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class OutboxProcessorOptions
{
    /// <summary>
    /// Defines the maximum number of outbox messages to process in a single batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Specifies the duration to wait between consecutive attempts to process outbox messages.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(10);
}

internal sealed class OutboxProcessor : BackgroundService
{
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IOptionsMonitor<OutboxProcessorOptions> _optionsMonitor;
    private readonly IServiceProvider _services;

    public OutboxProcessor(ILogger<OutboxProcessor> logger, IOptionsMonitor<OutboxProcessorOptions> optionsMonitor, IServiceProvider services)
    {
        _logger = logger;
        _optionsMonitor = optionsMonitor;
        _services = services;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();
            var options = _optionsMonitor.CurrentValue;
            
            _logger.LogInformation("Processing outbox...");
            
            await using var transaction = await dbContext.BeginTransactionAsync(stoppingToken);
        
            var messages = (await dbContext.Connection.QueryAsync(
                """
                select id as "Id", type as "Type", payload as "Payload"
                from outbox_messages
                where processed_on_utc is null
                order by created_on_utc limit @limit
                """,
                new { limit = options.BatchSize },
                transaction)).ToList();
        
            _logger.LogInformation("Found {MessageCount} messages to process.", messages.Count);
            
            foreach (var message in messages)
            {
                Guid id = message.Id;
                string type = message.Type;
                string payload = message.Payload;
                
                try
                {
                    var messageType = typeof(SharedKernel).Assembly.GetType(type)!;
                    var deserializedMessage = JsonSerializer.Deserialize(payload, messageType)!;

                    await bus.Publish(deserializedMessage, stoppingToken);

                    await dbContext.Connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, id },
                        transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message with ID {OutboxMessageId}", id);

                    await dbContext.Connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc, error_message = @Error
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, Error = ex.Message, Id = id },
                        transaction);
                }
            }

            await transaction.CommitAsync(stoppingToken);

            _logger.LogInformation("Finished processing outbox.");
            
            await Task.Delay(options.Delay, stoppingToken);
        }
    }
}
