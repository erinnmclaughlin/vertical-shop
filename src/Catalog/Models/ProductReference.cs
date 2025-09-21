namespace VerticalShop.Catalog;

/// <summary>
/// Provides a simplified representation of a product for API consumers,
/// containing only the fields that are relevant for external interactions.
/// </summary>
public sealed record ProductReference
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
}
