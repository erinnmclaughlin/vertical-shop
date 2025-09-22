using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog.Features.ListProducts;

internal sealed class ListProductsRequestHandler : IRequestHandler<ListProductsRequest, Ok<List<ProductReference>>>
{
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 20;

    private readonly IListProductsDataService _dataSource;

    public ListProductsRequestHandler(IListProductsDataService dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Ok<List<ProductReference>>> Handle(ListProductsRequest query, CancellationToken cancellationToken = default)
    {
        var products = await _dataSource.ListProductsAsync(query.Offset ?? DefaultOffset, query.Limit ?? DefaultLimit, cancellationToken);
        return TypedResults.Ok(products);
    }
}
