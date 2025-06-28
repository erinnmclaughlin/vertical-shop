using System.Data;
using System.Text.Json;
using ContextDrivenDevelopment.Api.Persistence.Postgres;
using Npgsql;
using NpgsqlTypes;

namespace ContextDrivenDevelopment.Api.Messaging;

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

public sealed class PostgresOutbox : IOutbox
{
    private readonly PostgresUnitOfWork _unitOfWork;
    
    public PostgresOutbox(PostgresUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    /// <inheritdoc />
    public async Task InsertMessage<T>(T message, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           insert into outbox_messages(id, type, payload)
                           values (@id, @type, @payload::jsonb)
                           """;

        var parameters = new
        {
            id = Guid.CreateVersion7(),
            type = typeof(T).Name,
            payload = JsonSerializer.Serialize(message)
        };
        
        await _unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<OutboxMessage<T>?> GetNextMessageOfType<T>(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           select id, type, payload, created_at
                           from outbox_messages 
                           where type = @type limit 1
                           """;
        
        var result = await _unitOfWork.QueryFirstOrDefaultAsync<dynamic>(sql, new { type = typeof(T).Name }, cancellationToken);

        if (result is null)
            return null;

        return new OutboxMessage<T>
        {
            Id = result.id,
            Type = result.type,
            Message = JsonSerializer.Deserialize<T>(result.payload),
            CreatedAt = result.created_at
        };
    }
}