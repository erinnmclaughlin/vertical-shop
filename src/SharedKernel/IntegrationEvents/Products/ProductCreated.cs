namespace VerticalShop.IntegrationEvents.Products;

/// <summary>
/// An integration event that is published when a new product is created.
/// </summary>
public sealed record ProductCreated(Guid ProductId, string ProductSlug, string ProductName);
