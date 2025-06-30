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

    /// <summary>
    /// The current product price, or null if the product is not available for sale.
    /// </summary>
    public decimal? Price { get; set; }
    
    /// <summary>
    /// A collection of attributes associated with the product, defining additional details or characteristics.
    /// </summary>
    public Dictionary<string, string> Attributes { get; init; } = [];
}
