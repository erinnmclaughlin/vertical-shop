using Dapper;

namespace VerticalShop.Products;

/// <summary>
/// Defines the contract for a data store that manages product entities.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Creates a new product in the data store.
    /// </summary>
    /// <param name="product">The product entity to be created.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of products from the data store.
    /// </summary>
    /// <param name="offset">The number of products to skip before starting to collect the result set.</param>
    /// <param name="limit">The maximum number of products to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of products.</returns>
    Task<List<Product>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product from the data store by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the product if found, or null if not.</returns>
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product by its unique slug identifier.
    /// </summary>
    /// <param name="slug">The slug that uniquely identifies the product.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the product if found; otherwise, null.</returns>
    Task<Product?> GetBySlugAsync(ProductSlug slug, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
internal sealed class ProductRepository(IDatabaseContext dbContext) : IProductRepository
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