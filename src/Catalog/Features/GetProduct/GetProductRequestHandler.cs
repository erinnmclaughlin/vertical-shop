using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using VerticalShop.Catalog.Models;

namespace VerticalShop.Catalog.Features.GetProduct;

internal sealed class GetProductRequestHandler : IRequestHandler<GetProductRequest, Results<Ok<ProductDetail>, NotFound>>
{
    private readonly INpgsqlDataStore _dataSource;

    public GetProductRequestHandler(INpgsqlDataStore dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Results<Ok<ProductDetail>, NotFound>> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var (identifierType, identifier) = ParseIdentifier(request);

        var result = (await _dataSource.QueryAsync(
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
            where p.{identifierType.ToString().ToLower()} = @identifier
            """,
            new { identifier },
            cancellationToken
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

    private static (ProductIdentifierType IdentifierType, dynamic Identifier) ParseIdentifier(GetProductRequest request)
    {
        if (request.IdentifierType is null)
        {
            if (Guid.TryParse(request.Identifier, out var guid))
            {
                return (ProductIdentifierType.Id, guid);
            }
            else
            {
                return (ProductIdentifierType.Slug, request.Identifier);
            }
        }
        else if (request.IdentifierType is ProductIdentifierType.Id)
        {
            return (ProductIdentifierType.Id, Guid.Parse(request.Identifier));
        }
        else
        {
            return (ProductIdentifierType.Slug, request.Identifier);
        }
    }
}