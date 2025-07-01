namespace VerticalShop.Catalog;

/// <summary>
/// Represents a product entity in the VerticalShop system.
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
    /// <remarks>
    /// Note that this value cannot be changed after the product is persisted.
    /// </remarks>
    public required ProductSlug Slug { get; init; }
    
    /// <summary>
    /// The product name.
    /// </summary>
    public required string Name { get; set; }
}
