namespace ContextDrivenDevelopment.Api.Domain.Products.Events;

/// <summary>
/// An integration event that is published when a new product is created.
/// </summary>
/// <param name="ProductSlug">The pruduct slug.</param>
public sealed record ProductCreated(string ProductSlug);
