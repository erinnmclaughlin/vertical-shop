using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog.Features.CreateProduct;

/// <summary>
/// A request to create a new product.
/// </summary>
public sealed record CreateProductRequest : IRequest<Results<Created, ValidationProblem, Conflict>>
{
    /// <summary>
    /// The unique, URL-friendly product identifier.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// The product name.
    /// </summary>
    public required string Name { get; init; }
}
