using Dapper;

namespace VerticalShop.Products;

/// <inheritdoc />
internal sealed class PostgresProductRepository(IDatabaseContext dbContext) : IProductRepository
{
    private readonly IDatabaseContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await _dbContext.Connection.ExecuteAsync(
            "insert into products.products (id, name, slug) values (@id, @name, @slug)", 
            new
            {
                id = product.Id.Value, 
                name = product.Name,
                slug = product.Slug.Value
            },
            _dbContext.CurrentTransaction
        );

        if (product.Attributes is { Count: > 0 } attributes)
        {
            var productId = await _dbContext.Connection.QuerySingleAsync<string>(
                "select id from products.products where slug = @slug",
                new { slug = product.Slug.Value },
                _dbContext.CurrentTransaction
            );
            
            await _dbContext.Connection.ExecuteAsync(
                "insert into products.product_attributes (id, product_id, name, value) values (@id, @productId, @name, @value)", 
                attributes.Select(x => new
                {
                    id = Guid.CreateVersion7(), 
                    productId, 
                    name = x.Key,
                    value = x.Value
                }),
                _dbContext.CurrentTransaction
            );
        }
    }

    /// <inheritdoc />
    public async Task<List<Product>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var results = (await _dbContext.Connection.QueryAsync(
            """
            select 
                p.id as "Id", 
                p.slug as "Slug", 
                p.name as "Name", 
                pa.name as "AttributeKey", 
                pa.value as "AttributeValue"
            from products.products p
            left join products.product_attributes pa on p.id = pa.product_id
            offset @offset
            limit @limit
            """,
            new { offset, limit },
            _dbContext.CurrentTransaction
        )).ToList();

        if (results.Count == 0)
            return [];

        var products = new List<Product>();
        
        foreach (var (_, values) in results.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList()))
        {
            var firstResult = values.First();
            
            products.Add(new Product
            {
                Id = firstResult.Id,
                Slug = firstResult.Slug,
                Name = firstResult.Name,
                Attributes = values
                    .Where(x => x.AttributeKey != null)
                    .GroupBy(x => (string)x.AttributeKey)
                    .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(v => v.AttributeValue)))
            });
        }

        return products;
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var results = (await _dbContext.Connection.QueryAsync(
            """
            select 
                p.id as "Id", 
                p.slug as "Slug", 
                p.name as "Name", 
                pa.name as "AttributeKey", 
                pa.value as "AttributeValue"
            from products.products p
            left join products.product_attributes pa on p.id = pa.product_id
            where p.id = @id
            """,
            new { id = id.Value },
            _dbContext.CurrentTransaction
        )).ToList();

        var firstResult = results.FirstOrDefault();

        if (firstResult is null)
            return null;
        
        return new Product
        {
            Id = firstResult.Id,
            Slug = firstResult.Slug,
            Name = firstResult.Name,
            Attributes = results
                .Where(x => x.AttributeKey != null)
                .GroupBy(x => (string)x.AttributeKey)
                .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(v => v.AttributeValue)))
        };
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetBySlugAsync(ProductSlug slug, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var results = (await _dbContext.Connection.QueryAsync(
            """
            select 
                p.id as "Id", 
                p.slug as "Slug", 
                p.name as "Name", 
                pa.name as "AttributeKey", 
                pa.value as "AttributeValue"
            from products.products p
            left join products.product_attributes pa on p.id = pa.product_id
            where p.slug = @slug
            """,
            new { slug = slug.Value },
            _dbContext.CurrentTransaction
        )).ToList();

        var firstResult = results.FirstOrDefault();

        if (firstResult is null)
            return null;
        
        var attributes = results
            .Where(x => x.AttributeKey != null)
            .GroupBy(x => (string)x.AttributeKey)
            .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(v => v.AttributeValue)));
        
        return new Product
        {
            Id = firstResult.Id,
            Slug = firstResult.Slug,
            Name = firstResult.Name,
            Attributes = attributes
        };
    }
}