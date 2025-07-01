using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for retrieving product details.
/// </summary>
public static class GetProduct
{
    public sealed record Query(string Identifier, ProductIdentifierType IdentifierType) : IRequest<Results<Ok<ProductDto>, NotFound>>;
    
    internal sealed class QueryHandler(IDatabaseContext dbContext) : IRequestHandler<Query, Results<Ok<ProductDto>, NotFound>>
    {
        public async Task<Results<Ok<ProductDto>, NotFound>> Handle(
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
