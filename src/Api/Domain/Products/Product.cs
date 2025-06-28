namespace ContextDrivenDevelopment.Api.Domain.Products;

/// <summary>
/// Represents a product.
/// </summary>
public sealed class Product
{
    /// <summary>
    /// The system-generated unique identifier for the product.
    /// </summary>
    public ProductId Id { get; init; } = ProductId.CreateNew();

    /// <summary>
    /// A unique, URL-friendly identifier representing the product.
    /// </summary>
    public required ProductSlug Slug { get; init; }
    
    /// <summary>
    /// The product name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A collection of attributes associated with the product, defining additional details or characteristics.
    /// </summary>
    public Dictionary<string, string> Attributes { get; init; } = [];
}
