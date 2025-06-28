namespace ContextDrivenDevelopment.Api.Domain.Products.Events;

public sealed record ProductCreated
{
    public required string ProductSlug { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
