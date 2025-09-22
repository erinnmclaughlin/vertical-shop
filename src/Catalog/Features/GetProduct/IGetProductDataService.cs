using OneOf;
using OneOf.Types;
using VerticalShop.Catalog.Models;

namespace VerticalShop.Catalog.Features.GetProduct;

internal interface IGetProductDataService
{
    Task<OneOf<ProductDetail, NotFound>> GetProductAsync(OneOf<Guid, string> identifier, CancellationToken cancellationToken = default);
}

internal sealed class GetProductNpgsqlDataService(INpgsqlDataStore dataSource) : IGetProductDataService
{
    public async Task<OneOf<ProductDetail, NotFound>> GetProductAsync(OneOf<Guid, string> identifier, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteQueryAsync(identifier, identifier.IsT0 ? ProductIdentifierType.Id : ProductIdentifierType.Slug, cancellationToken);

        if (result.Count == 0)
            return new NotFound();

        var productVariants = new List<ProductDetail.ProductVariant>();

        foreach (var item in result.Where(x => x.VariantId is not null))
        {
            var variant = new ProductDetail.ProductVariant
            {
                Id = item.VariantId,
                Name = item.VariantName,
                Attributes = new Dictionary<string, string>()
            };

            if (item.VariantAttributes is string variantAttributes && !string.IsNullOrEmpty(variantAttributes))
            {
                var attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(variantAttributes);
                if (attributes is not null)
                {
                    variant = variant with { Attributes = attributes };
                }
            }

            productVariants.Add(variant);
        }

        return new ProductDetail
        {
            Id = result[0].Id,
            Slug = result[0].Slug,
            Name = result[0].Name,
            Variants = productVariants
        };
    }

    private async Task<List<dynamic>> ExecuteQueryAsync(OneOf<Guid, string> identifier, ProductIdentifierType identifierType, CancellationToken cancellationToken)
    {
        var items = await dataSource.QueryAsync(
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
            new { identifier.Value },
            cancellationToken
        );

        return [.. items];
    }
}
