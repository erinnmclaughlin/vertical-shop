using Dapper;
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
    /// Handles the process of querying the inventory for available quantity of a product.
    /// </summary>
    public sealed class QueryHandler(NpgsqlDataSource dataSource)
    {
        private readonly NpgsqlDataSource _dataSource = dataSource;

        /// <summary>
        /// Handles the process of retrieving the quantity of a product available in inventory.
        /// </summary>
        /// <param name="productSlug">The unique identifier or slug of the product to check in the inventory.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation if necessary.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is a <c>Result</c> object containing an <c>Ok</c> result with the quantity available if the product is found, or a <c>NotFound</c> result otherwise.
        /// </returns>
        public async Task<Result> Handle(string productSlug, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await using var connection = _dataSource.CreateConnection();
            var quantity = await connection.QuerySingleOrDefaultAsync<int?>(
                """
                select quantity
                from inventory.items
                where product_slug = @productSlug
                """,
                new { productSlug }
            );
            
            return quantity is null ? TypedResults.NotFound() : TypedResults.Ok(quantity.Value);
        }
    }
}
