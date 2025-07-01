using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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
    
    internal sealed class QueryHandler(IDatabaseContext dbContext) : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(
            Query query,
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
                where p.{query.IdentifierType.ToString().ToLower()} = @Identifier
                """,
                new { query.Identifier }
            );

            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        }
    }
}
