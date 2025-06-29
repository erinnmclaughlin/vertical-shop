namespace ContextDrivenDevelopment.Api.Messaging;

public sealed record OutboxMessage<T>
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Type { get; private init; } = typeof(T).FullName ?? typeof(T).Name;
    public required T Message { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
