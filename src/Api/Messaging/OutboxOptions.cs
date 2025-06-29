namespace VerticalShop.Api.Messaging;

/// <summary>
/// Configuration options for outbox processing.
/// </summary>
public sealed class OutboxOptions
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
