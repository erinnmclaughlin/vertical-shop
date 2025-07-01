using System.Diagnostics.CodeAnalysis;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for getting a paginated list of products.
/// </summary>
public static class ListProducts
{
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 20;

    /// <summary>
    /// Represents the parameters for querying a list of products, such as pagination options.
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public sealed record Query : IRequest<Ok<List<ProductDto>>>
    {
        /// <summary>
        /// The offset used for paginating query results.
        /// </summary>
        /// <remarks>
        /// The offset determines the starting point in the list of products from which results are returned.
        /// If not specified, it defaults to 0, meaning the query will start from the beginning of the list.
        /// </remarks>
        public int? Offset { get; init; } = DefaultOffset;

        /// <summary>
        /// The maximum number of products to return in the query results.
        /// </summary>
        /// <remarks>
        /// Limit specifies the upper bound on the number of products that should be retrieved in a single query.
        /// If not provided, a default value is applied to prevent excessive data retrieval. It is used in conjunction
        /// with the <see cref="Offset"/> property to support pagination when listing products.
        /// </remarks>
        public int? Limit { get; init; } = DefaultLimit;
    }

    /// <summary>
    /// Handles queries related to retrieving a list of products.
    /// </summary>
    public sealed class QueryHandler(NpgsqlDataSource dataSource) : IRequestHandler<Query, Ok<List<ProductDto>>>
    {
        /// <summary>
        /// Handles the product list query operation, retrieving a list of products based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query object containing parameters for offset and limit.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing an HTTP result wrapping a list of <see cref="ProductDto"/> objects.</returns>
        public async Task<Ok<List<ProductDto>>> Handle(Query query, CancellationToken cancellationToken = default)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            
            var products = await connection.QueryAsync<ProductDto>(
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
            );

            return TypedResults.Ok(products.ToList());
        }
    }
}