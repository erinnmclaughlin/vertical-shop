using System.Diagnostics.CodeAnalysis;

namespace VerticalShop.Api.Products;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed record ProductDto
{
    /// <summary>
    /// The unique identifier of the product.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The unique URL-friendly identifier for a product.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// The name of the product.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The attributes associated with the product as key-value pairs.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Attributes { get; init; }

    /// <summary>
    /// Creates a new instance of <see cref="ProductDto"/> from a given <see cref="Product"/>.
    /// </summary>
    /// <param name="product">The product entity from which to create the DTO.</param>
    /// <returns>An instance of <see cref="ProductDto"/> populated with data from the provided product.</returns>
    public static ProductDto FromProduct(Product product) => new()
    {
        Id = product.Id,
        Slug = product.Slug,
        Name = product.Name,
        Attributes = product.Attributes
    };
}
