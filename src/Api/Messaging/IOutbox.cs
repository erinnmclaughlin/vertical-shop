namespace VerticalShop.Api.Messaging;

public interface IOutbox
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
    
    /// <summary>
    /// Retrieves the next message from the outbox of the specified type, if available.
    /// </summary>
    /// <typeparam name="T">The type of the message to be retrieved.</typeparam>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the next message of type <typeparamref name="T"/> in the outbox, or null if no such message is found.</returns>
    Task<OutboxMessage<T>?> GetNextMessageOfType<T>(CancellationToken cancellationToken = default);
}