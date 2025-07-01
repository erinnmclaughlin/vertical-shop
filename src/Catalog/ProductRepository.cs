using Dapper;

namespace VerticalShop.Catalog;

/// <summary>
/// Defines the contract for a data store that manages product entities.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Inserts a new product into the data store.
    /// </summary>
    /// <param name="product">The product entity to create.</param>
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
            "insert into catalog.products (id, name, slug) values (@id, @name, @slug)", 
            new
            {
                id = product.Id.Value,
                name = product.Name,
                slug = product.Slug.Value
            },
            _dbContext.CurrentTransaction
        );
    }

    /// <inheritdoc />
    public async Task<List<Product>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = await _dbContext.Connection.QueryAsync<ProductTable>(
            """
            select 
                p.id as "Id", 
                p.name as "Name",
                p.slug as "Slug"
            from catalog.products p
            offset @offset
            limit @limit
            """,
            new { offset, limit },
            _dbContext.CurrentTransaction
        );

        return results.Select(r => r.ToProduct()).ToList();
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _dbContext.Connection.QuerySingleOrDefaultAsync<ProductTable>(
            """
            select 
                p.id as "Id", 
                p.slug as "Slug", 
                p.name as "Name"
            from catalog.products p
            where p.id = @id
            """,
            new { id = id.Value },
            _dbContext.CurrentTransaction
        );

        return result?.ToProduct();
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetBySlugAsync(ProductSlug slug, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _dbContext.Connection.QuerySingleOrDefaultAsync<ProductTable>(
            """
            select 
                p.id as "Id", 
                p.slug as "Slug", 
                p.name as "Name"
            from catalog.products p
            where p.slug = @slug
            """,
            new { slug = slug.Value },
            _dbContext.CurrentTransaction
        );

        return result?.ToProduct();
    }
    
    private sealed record ProductTable
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Slug { get; init; }
        
        public Product ToProduct() => new()
        {
            Id = new ProductId(Id),
            Slug = ProductSlug.Parse(Slug),
            Name = Name
        };
        
        public static implicit operator Product(ProductTable productTable) => productTable.ToProduct();
    }
}
