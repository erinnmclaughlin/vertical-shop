namespace VerticalShop.IntegrationEvents.Products;

/// <summary>
/// An integration event that is published when a new product is created.
/// </summary>
/// <param name="ProductSlug">The product slug.</param>
public sealed record ProductCreated(string ProductId, string ProductSlug, string ProductName);
