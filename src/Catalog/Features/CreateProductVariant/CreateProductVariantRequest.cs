using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog.Features.CreateProductVariant;

/// <summary>
/// A request to create a new product variant.
/// </summary>
public sealed record CreateProductVariantRequest : IRequest<Results<Created, ValidationProblem>>
{
    /// <summary>
    /// The ID of the product to which the variant belongs.
    /// </summary>
    public required Guid ProductId { get; init; }

    /// <summary>
    /// The name of the product variant.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A dictionary of attributes associated with the product variant.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Attributes { get; init; }
}
