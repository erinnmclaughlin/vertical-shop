using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Dapper;
using FluentMigrator.Runner;
using MassTransit;
using Microsoft.Extensions.Options;
using Npgsql;

namespace VerticalShop.OutboxProcessor;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
internal sealed record OutboxProcessorOptions
{
    public int BatchSize { get; init; } = 100;
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(10);
}

internal sealed record OutboxMessage
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Payload { get; init; }
}

internal sealed class Worker(
    NpgsqlDataSource dataSource,
    ILogger<Worker> logger,
    IOptionsMonitor<OutboxProcessorOptions> optionsMonitor,
    IServiceProvider services
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Processing outbox...");

            using var scope = services.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();
            var options = optionsMonitor.CurrentValue;

            await using var connection = await dataSource.OpenConnectionAsync(stoppingToken);
            await using var transaction = await connection.BeginTransactionAsync(stoppingToken);

            var messages =
                (await connection.QueryAsync<OutboxMessage>(
                    """
                    select id as "Id", type as "Type", payload as "Payload"
                    from outbox_messages
                    where processed_on_utc is null
                    order by created_on_utc limit @limit
                    """,
                    new { limit = options.BatchSize },
                    transaction)).ToList();

            logger.LogInformation("Found {MessageCount} messages to process.", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    // note: this requires all messages to be defined in the "SharedKernel" assembly
                    // for now, this is ok. eventually, this may need to be more flexible
                    var messageType = typeof(SharedKernel).Assembly.GetType(message.Type)!;
                    var deserializedMessage = JsonSerializer.Deserialize(message.Payload, messageType)!;

                    await bus.Publish(deserializedMessage, stoppingToken);

                    await connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, message.Id },
                        transaction);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process message with ID {OutboxMessageId}", message.Id);

                    await connection.ExecuteAsync(
                        """
                        UPDATE outbox_messages
                        SET processed_on_utc = @ProcessedOnUtc, error_message = @Error
                        WHERE id = @Id
                        """,
                        new { ProcessedOnUtc = DateTimeOffset.UtcNow, Error = ex.Message, message.Id },
                        transaction);
                }
            }

            await transaction.CommitAsync(stoppingToken);

            logger.LogInformation("Finished processing outbox.");

            await Task.Delay(options.Delay, stoppingToken);
        }
    }
}
