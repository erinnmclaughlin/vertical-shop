using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog.Features.ListProducts;

/// <summary>
/// A request to get a paginated list of products.
/// </summary>
public sealed record ListProductsRequest : IRequest<Ok<List<ProductReference>>> // todo: return pagination metadata with result
{
    /// <summary>
    /// The offset used for paginating query results.
    /// </summary>
    public int? Offset { get; init; }

    /// <summary>
    /// The maximum number of products to return in the query results.
    /// </summary>
    public int? Limit { get; init; }
}
