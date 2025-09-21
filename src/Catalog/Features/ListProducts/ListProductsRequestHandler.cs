using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace VerticalShop.Catalog.Features.ListProducts;

internal sealed class ListProductsRequestHandler : IRequestHandler<ListProductsRequest, Ok<IReadOnlyList<ProductReference>>>
{
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 20;

    private readonly NpgsqlDataSource _dataSource;

    public ListProductsRequestHandler(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Ok<IReadOnlyList<ProductReference>>> Handle(ListProductsRequest query, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        IReadOnlyList<ProductReference> products = (await connection.QueryAsync<ProductReference>(
            """
                select 
                    p.id as "Id", 
                    p.name as "Name",
                    p.slug as "Slug"
                from catalog.products p
                offset @offset
                limit @limit
                """,
            new { offset = query.Offset ?? DefaultOffset, limit = query.Limit ?? DefaultLimit }
        )).ToList();

        return TypedResults.Ok(products);
    }

}