using ContextDrivenDevelopment.Api.Persistence.Postgres;

namespace ContextDrivenDevelopment.Api.Domain.Products.Persistence;

/// <inheritdoc />
internal sealed class PostgresProductRepository : IProductRepository
{
    private readonly PostgresUnitOfWork _unitOfWork;

    public PostgresProductRepository(PostgresUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO "products".products (id, name, slug)
            VALUES (@id, @name, @slug)
            """;
        
        await _unitOfWork.ExecuteAsync(sql, product, cancellationToken);

        if (product.Attributes is { Count: > 0 } attributes)
        {
            var productId = await _unitOfWork.QuerySingleAsync<Guid>(
                "select id from products.products where slug = @slug",
                new { product.Slug }, 
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
    public async Task<OneOf<Product, NotFound>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM \"products\".products WHERE id = @id";
        return await GetAsync(sql, new { id }, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<OneOf<Product, NotFound>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string getProductSql = "SELECT * FROM \"products\".products WHERE slug = @slug";
        return await GetAsync(getProductSql, new { slug }, cancellationToken);
    }

    private async Task<OneOf<Product, NotFound>> GetAsync(string sql, object parameters, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.QuerySingleOrDefaultAsync<Product>(sql, parameters, cancellationToken);
        
        if (product is null)
            return new NotFound();
        
        var productAttributes = await _unitOfWork.QueryListAsync(
            "SELECT name, value FROM products.product_attributes WHERE product_id = @id",
            new { id = product.Id },
            cancellationToken
        );
        
        var attributes = productAttributes
            .Select(x => new { Key = (string)x.name, Value = (string)x.value })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(v => v.Value)));
        
        foreach (var (key, value) in attributes)
            product.Attributes.Add(key, value);
        
        return product;
    }
}
