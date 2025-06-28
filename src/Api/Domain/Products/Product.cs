namespace ContextDrivenDevelopment.Api.Domain.Products;

/// <summary>
/// Represents a product.
/// </summary>
public sealed class Product
{
    /// <summary>
    /// The system-generated unique identifier for the product.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// A unique, URL-friendly identifier representing the product.
    /// </summary>
    public required string Slug { get; init; }
    
    /// <summary>
    /// The product name.
    /// </summary>
    public required string Name { get; set; }
}
