using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using System.Text.Json;
using VerticalShop.Catalog.Models;

namespace VerticalShop.Catalog;

using Result = Results<Ok<ProductDetail>, NotFound>;

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

            dynamic identifier = query.IdentifierType switch
            {
                ProductIdentifierType.Id => Guid.Parse(query.Identifier),
                ProductIdentifierType.Slug => query.Identifier
            };

            var result = (await connection.QueryAsync(
                $"""
                select 
                    p.id as "Id", 
                    p.slug as "Slug", 
                    p.name as "Name",
                    pv.id as "VariantId",
                    pv.name as "VariantName",
                    pv.attributes as "VariantAttributes"
                from catalog.products p
                left join catalog.product_variants pv on pv.product_id = p.id
                where p.{query.IdentifierType.ToString().ToLower()} = @identifier
                """,
                new { identifier }
            )).ToList();

            if (result.Count == 0)
                return TypedResults.NotFound();


            var productVariants = new List<ProductDetail.ProductVariant>();

            foreach (var item in result)
            {
                if (item.VariantId is not null && item.VariantName is not null)
                {
                    var attributes = new Dictionary<string, string>();

                    if (item.VariantAttributes is string variantAttributes && !string.IsNullOrEmpty(variantAttributes))
                    {
                        var deserializedAttributes = JsonSerializer.Deserialize<Dictionary<string, string>>(variantAttributes);
                        if (deserializedAttributes is not null)
                        {
                            attributes = deserializedAttributes;
                        }
                    }

                    productVariants.Add(new ProductDetail.ProductVariant
                    {
                        Id = item.VariantId,
                        Name = item.VariantName,
                        Attributes = attributes
                    });
                }
            }

            return TypedResults.Ok(new ProductDetail
            {
                Id = result[0].Id,
                Slug = result[0].Slug,
                Name = result[0].Name,
                Variants = productVariants
            });
        }
    }
}
