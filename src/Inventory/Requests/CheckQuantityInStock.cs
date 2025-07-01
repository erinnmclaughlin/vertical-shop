using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace VerticalShop.Inventory;

using Result = Results<Ok<int>, NotFound>;

/// <summary>
/// Provides functionality to check the available quantity of a product in stock.
/// </summary>
public static class CheckQuantityInStock
{
    /// <summary>
    /// A request to check the quantity of a product in stock by its slug.
    /// </summary>
    /// <param name="ProductSlug">The product slug.</param>
    public sealed record Query(string ProductSlug) : IRequest<Result>;
    
    internal sealed class QueryHandler(NpgsqlDataSource dataSource) : IRequestHandler<Query, Result>
    {
        private readonly NpgsqlDataSource _dataSource = dataSource;

        public async Task<Result> Handle(Query query, CancellationToken cancellationToken = default)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var quantity = await connection.QuerySingleOrDefaultAsync<int?>(
                """
                select quantity
                from inventory.items
                where product_slug = @ProductSlug
                """,
                new { query.ProductSlug }
            );
            
            return quantity is null ? TypedResults.NotFound() : TypedResults.Ok(quantity.Value);
        }
    }
}
