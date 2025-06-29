namespace ContextDrivenDevelopment.Api.Messaging;

public sealed class PostgresOutbox : IOutbox
{
    private readonly NpgsqlConnection _connection;
    
    public PostgresOutbox(NpgsqlConnection connection)
    {
        _connection = connection;
    }
    
    /// <inheritdoc />
    public async Task InsertMessage<T>(T message, CancellationToken cancellationToken = default)
    {
        await _connection.ExecuteAsync(
            "insert into outbox_messages(id, type, payload) values (@id, @type, @payload::jsonb)", 
            new
            {
                id = Guid.CreateVersion7(),
                type = typeof(T).FullName,
                payload = JsonSerializer.Serialize(message)
            }
        );
    }
    
    /// <inheritdoc />
    public async Task<OutboxMessage<T>?> GetNextMessageOfType<T>(CancellationToken cancellationToken = default)
    {
        var result = await _connection.QueryFirstOrDefaultAsync(
            """
            select id, payload, created_on_utc
            from outbox_messages 
            where type = @type limit 1
            """,
            new { type = typeof(T).FullName }
        );

        return result is null ? null : new OutboxMessage<T>
        {
            Id = result.id,
            Message = JsonSerializer.Deserialize<T>(result.payload),
            CreatedAt = result.created_on_utc
        };
    }
}