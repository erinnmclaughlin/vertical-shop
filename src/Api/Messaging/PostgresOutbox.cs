using System.Text.Json;
using ContextDrivenDevelopment.Api.Persistence.Postgres;

namespace ContextDrivenDevelopment.Api.Messaging;

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
            type = typeof(T).FullName,
            payload = JsonSerializer.Serialize(message)
        };
        
        await _unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<OutboxMessage<T>?> GetNextMessageOfType<T>(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           select id, type, payload, created_on_utc
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