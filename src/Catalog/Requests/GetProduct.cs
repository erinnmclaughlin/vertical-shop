using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace VerticalShop.Catalog;

using Result = Results<Ok<ProductDto>, NotFound>;

/// <summary>
/// Provides the implementation for retrieving product details.
/// </summary>
public static class GetProduct
{
    /// <summary>
    /// Represents a request to retrieve a product by its identifier.
    /// </summary>
    /// <param name="Identifier">The product identifier.</param>
    /// <param name="IdentifierType">The identifier type (e.g., id or slug)</param>
    public sealed record Query(string Identifier, ProductIdentifierType IdentifierType) : IRequest<Result>;
    
    internal sealed class QueryHandler(NpgsqlDataSource dataSource) : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            
            var result = await connection.QuerySingleOrDefaultAsync<ProductDto>(
                $"""
                select 
                    p.id as "Id", 
                    p.slug as "Slug", 
                    p.name as "Name"
                from catalog.products p
                where p.{query.IdentifierType.ToString().ToLower()} = @Identifier
                """,
                new { query.Identifier }
            );

            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        }
    }
}
