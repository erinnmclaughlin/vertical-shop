namespace VerticalShop.Catalog.Models;

/// <summary>
/// Represents detailed information about a product.
/// variants.
/// </summary>
public sealed record ProductDetail
{
    /// <summary>
    /// The unique identifier of the product.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The unique URL-friendly identifier for a product.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// The name of the product.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The collection of available variants for the product.
    /// </summary>
    public required IReadOnlyList<ProductVariant> Variants { get; init; }

    /// <summary>
    /// Represents a specific variant of a product, identified by a unique identifier and name.
    /// </summary>
    public sealed record ProductVariant
    {
        /// <summary>
        /// The unique identifier of the product variant.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// The name of the product variant.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the collection of key-value pairs that represent additional attributes associated with this instance.
        /// </summary>
        public required IReadOnlyDictionary<string, string> Attributes { get; init; }
    }
}
