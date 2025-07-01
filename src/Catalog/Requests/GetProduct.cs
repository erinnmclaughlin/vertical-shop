using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for retrieving product details.
/// </summary>
public static class GetProduct
{
    /// <summary>
    /// Handles requests to get a specific product.
    /// </summary>
    public sealed class QueryHandler(IDatabaseContext dbContext)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="identifierType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Results<Ok<ProductDto>, NotFound>> GetProduct(
            string identifier,
            ProductIdentifierType identifierType, 
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await dbContext.Connection.QuerySingleOrDefaultAsync<ProductDto>(
                $"""
                select 
                    p.id as "Id", 
                    p.slug as "Slug", 
                    p.name as "Name"
                from catalog.products p
                where p.{identifierType.ToString().ToLower()} = @identifier
                """,
                new { identifier }
            );

            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        }
    }
}
