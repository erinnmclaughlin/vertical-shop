namespace VerticalShop.Api.Messaging;

/// <summary>
/// Provides functionality for managing and processing messages in an outbox pattern.
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Inserts an outbox message into the data store for eventual processing or dispatch.
    /// </summary>
    /// <param name="message">
    /// The message object to be inserted into the outbox.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// </param>
    /// <typeparam name="T">
    /// The type of the message being inserted.
    /// </typeparam>
    /// <returns>
    /// A task that represents the asynchronous insert operation.
    /// </returns>
    Task InsertMessage<T>(T message, CancellationToken cancellationToken = default);
}