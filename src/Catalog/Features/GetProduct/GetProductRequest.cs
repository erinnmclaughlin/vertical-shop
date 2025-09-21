using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using VerticalShop.Catalog.Models;

namespace VerticalShop.Catalog.Features.GetProduct;

/// <summary>
/// A request to get a product by its identifier.
/// </summary>
public sealed record GetProductRequest : IRequest<Results<Ok<ProductDetail>, NotFound>>
{
    /// <summary>
    /// The product identifier.
    /// </summary>
    public required string Identifier { get; init; }

    /// <summary>
    /// The identifier type (e.g., id or slug). If not specified, it will be inferred based on the format of the Identifier.
    /// </summary>
    public ProductIdentifierType? IdentifierType { get; init; }
}
