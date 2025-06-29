namespace ContextDrivenDevelopment.Api.Domain.Products;

/// <inheritdoc />
internal sealed class PostgresProductRepository : IProductRepository
{
    private readonly NpgsqlConnection _connection;

    public PostgresProductRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await _connection.ExecuteAsync(
            "insert into products.products (id, name, slug) values (@id, @name, @slug)", 
            new
            {
                id = product.Id.Value, 
                name = product.Name,
                slug = product.Slug.Value
            }
        );

        if (product.Attributes is { Count: > 0 } attributes)
        {
            var productId = await _connection.QuerySingleAsync<string>(
                "select id from products.products where slug = @slug",
                new { slug = product.Slug.Value }
            );
            
            await _connection.ExecuteAsync(
                "insert into products.product_attributes (id, product_id, name, value) values (@id, @productId, @name, @value)", 
                attributes.Select(x => new
                {
                    id = Guid.CreateVersion7(), 
                    productId, 
                    name = x.Key,
                    value = x.Value
                })
            );
        }
    }
    
    /// <inheritdoc />
    public async Task<OneOf<Product, NotFound>> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        const string sql = "SELECT name, slug FROM products.products WHERE id = @id";
        var getProductResult = await _connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { id = id.Value });
        
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
        cancellationToken.ThrowIfCancellationRequested();
        
        const string sql = "SELECT id, name FROM products.products WHERE slug = @slug";
        var getProductResult = await _connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { slug = slug.Value });
        
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
        cancellationToken.ThrowIfCancellationRequested();
        
        var productAttributes = await _connection.QueryAsync(
            "SELECT name, value FROM products.product_attributes WHERE product_id = @id",
            new { id = productId.Value }
        );
        
        return productAttributes
            .Select(x => new { Key = (string)x.name, Value = (string)x.value })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(v => v.Value)));
    }
}