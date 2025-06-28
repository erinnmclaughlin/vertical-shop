using ContextDrivenDevelopment.Api.Persistence.Postgres;

namespace ContextDrivenDevelopment.Api.Domain.Products;

/// <inheritdoc />
internal sealed class PostgresProductRepository : IProductRepository
{
    private readonly PostgresUnitOfWork _unitOfWork;

    public PostgresProductRepository(PostgresUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO products.products (id, name, slug)
            VALUES (@id, @name, @slug)
            """;

        var parameters = new { id = product.Id.Value, name = product.Name, slug = product.Slug.Value };
        await _unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);

        if (product.Attributes is { Count: > 0 } attributes)
        {
            
            var productId = await _unitOfWork.QuerySingleAsync<string>(
                "select id from products.products where slug = @slug",
                new { slug = product.Slug.Value }, 
                cancellationToken
            );

            const string attributeSql =
                """
                insert into products.product_attributes (id, product_id, name, value)
                values (@id, @productId, @name, @value)
                """;
        
            var attributesToInsert = attributes.Select(x => new { id = Guid.CreateVersion7(), productId, name = x.Key, value = x.Value });
            await _unitOfWork.ExecuteAsync(attributeSql, attributesToInsert, cancellationToken);
        }
    }
    
    /// <inheritdoc />
    public async Task<OneOf<Product, NotFound>> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT name, slug FROM products.products WHERE id = @id";
        var getProductResult = await _unitOfWork.QuerySingleOrDefaultAsync<dynamic>(sql, new { id = id.Value }, cancellationToken);
        
        if (getProductResult is null)
            return new NotFound();

        return new Product
        {
            Id = id,
            Slug = getProductResult.slug,
            Name = getProductResult.name,
            Attributes = await GetAttributesAsync(id, cancellationToken)
        };
    }
    
    /// <inheritdoc />
    public async Task<OneOf<Product, NotFound>> GetBySlugAsync(ProductSlug slug, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, name FROM products.products WHERE slug = @slug";
        var getProductResult = await _unitOfWork.QuerySingleOrDefaultAsync<dynamic>(sql, new { slug = slug.Value }, cancellationToken);
        
        if (getProductResult is null)
            return new NotFound();

        var productId = ProductId.Parse(getProductResult.id);
        
        return new Product
        {
            Id = productId,
            Slug = slug,
            Name = getProductResult.name,
            Attributes = await GetAttributesAsync(productId, cancellationToken)
        };
    }

    private async Task<Dictionary<string, string>> GetAttributesAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        var productAttributes = await _unitOfWork.QueryListAsync(
            "SELECT name, value FROM products.product_attributes WHERE product_id = @id",
            new { id = productId.Value },
            cancellationToken
        );
        
        return productAttributes
            .Select(x => new { Key = (string)x.name, Value = (string)x.value })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(v => v.Value)));
    }
}