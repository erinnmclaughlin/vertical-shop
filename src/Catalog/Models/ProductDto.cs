using System.Diagnostics.CodeAnalysis;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides a simplified representation of a product for API consumers,
/// containing only the fields that are relevant for external interactions.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed record ProductDto
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
    /// Creates a new instance of <see cref="ProductDto"/> from a given <see cref="Product"/>.
    /// </summary>
    /// <param name="product">The product entity from which to create the DTO.</param>
    /// <returns>An instance of <see cref="ProductDto"/> populated with data from the provided product.</returns>
    public static ProductDto FromProduct(Product product) => new()
    {
        Id = product.Id.Value,
        Slug = product.Slug,
        Name = product.Name
    };
}
