namespace VerticalShop.Api.Messaging;

internal sealed class PostgresOutbox : IOutbox
{
    private readonly NpgsqlConnection _connection;
    
    public PostgresOutbox(NpgsqlConnection connection)
    {
        _connection = connection;
    }
    
    /// <inheritdoc />
    public async Task InsertMessage<T>(T message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
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
}
