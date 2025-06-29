namespace VerticalShop.Api.Products;

/// <summary>
/// An integration event that is published when a new product is created.
/// </summary>
/// <param name="ProductSlug">The product slug.</param>
public sealed record ProductCreated(string ProductId, string ProductSlug, string ProductName)
{
    /// <summary>
    /// Creates a new <see cref="ProductCreated"/> instance from the specified <see cref="Product"/> instance.
    /// </summary>
    /// <param name="product">The product instance containing data for the new <see cref="ProductCreated"/> object.</param>
    /// <returns>A new <see cref="ProductCreated"/> instance populated with data from the specified product.</returns>
    public static ProductCreated FromProduct(Product product) => new(product.Id, product.Slug, product.Name);
}
