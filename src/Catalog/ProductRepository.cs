using Dapper;
using Microsoft.Extensions.Logging;

namespace VerticalShop.Catalog;

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

    /// <summary>
    /// Updates the price for a product in the data store.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to update.</param>
    /// <param name="price">The new price for the product.</param>
    /// <param name="validFrom">The date from which this price becomes valid.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdatePriceAsync(ProductId productId, decimal price, DateTimeOffset validFrom, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
internal sealed class ProductRepository(
    IDatabaseContext dbContext, 
    ILogger<ProductRepository> logger
) : IProductRepository
{
    private readonly IDatabaseContext _dbContext = dbContext;
    private readonly ILogger<ProductRepository> _logger = logger;

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

        if (product.Attributes is { Count: > 0 } attributes)
        {
            var productId = await _dbContext.Connection.QuerySingleAsync<string>(
                "select id from catalog.products where slug = @slug",
                new { slug = product.Slug.Value },
                _dbContext.CurrentTransaction
            );
            
            await _dbContext.Connection.ExecuteAsync(
                "insert into catalog.product_attributes (id, product_id, name, value) values (@id, @productId, @name, @value)", 
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
            from catalog.products p
            left join catalog.product_attributes pa on p.id = pa.product_id
            offset @offset
            limit @limit
            """,
            new { offset, limit },
            _dbContext.CurrentTransaction
        )).ToList();

        if (results.Count == 0)
            return [];

        var products = new List<Product>();

        var prices = await GetProductPrices(results.Select(x => (string)x.Id).Distinct().ToArray(), cancellationToken);
        
        foreach (var (_, values) in results.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList()))
        {
            var firstResult = values.First();
            
            products.Add(new Product
            {
                Id = firstResult.Id,
                Slug = firstResult.Slug,
                Name = firstResult.Name,
                Price = prices[firstResult.Id],
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
            from catalog.products p
            left join catalog.product_attributes pa on p.id = pa.product_id
            where p.id = @id
            """,
            new { id = id.Value },
            _dbContext.CurrentTransaction
        )).ToList();

        var firstResult = results.FirstOrDefault();

        if (firstResult is null)
            return null;
        
        var prices = await GetProductPrices([firstResult.Id], cancellationToken);
        
        return new Product
        {
            Id = firstResult.Id,
            Slug = firstResult.Slug,
            Name = firstResult.Name,
            Price = prices[firstResult.Id],
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
            from catalog.products p
            left join catalog.product_attributes pa on p.id = pa.product_id
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
        
        var prices = await GetProductPrices([firstResult.Id], cancellationToken);
        
        return new Product
        {
            Id = firstResult.Id,
            Slug = firstResult.Slug,
            Name = firstResult.Name,
            Attributes = attributes,
            Price = prices[firstResult.Id]
        };
    }

    private async Task<Dictionary<string, decimal?>> GetProductPrices(string[] productIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving product prices for {ProductIds} products", string.Join(", ", productIds));
        
        // todo: provide date from service
        var now = DateTimeOffset.UtcNow;

        var prices = await _dbContext.Connection.QueryAsync(
            """
            select product_id as "ProductId", price as "Price"
            from catalog.product_prices
            where product_id = any(@productIds) and valid_from <= @now and (valid_to is null or valid_to > @now)
            """,
            new { productIds, now },
            _dbContext.CurrentTransaction);
        
        return productIds.ToDictionary(x => x, x => (decimal?)prices.SingleOrDefault(y => y.ProductId == x)?.Price);
    }

    /// <inheritdoc />
    public async Task UpdatePriceAsync(ProductId productId, decimal price, DateTimeOffset validFrom, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // Check if there's already a price that would conflict with the new one
        var existingPrices = await _dbContext.Connection.QueryAsync(
            """
            select valid_from, valid_to 
            from catalog.product_prices 
            where product_id = @productId 
            and (
                (valid_from <= @validFrom and (valid_to is null or valid_to > @validFrom))
                or (valid_from < @validFrom and valid_to is null)
            )
            """,
            new { productId = productId.Value, validFrom = validFrom.UtcDateTime },
            _dbContext.CurrentTransaction
        );

        if (existingPrices.Any())
        {
            var existingPrice = existingPrices.First();
            var message = existingPrice.valid_to == null
                ? $"A price is already set for this product from {existingPrice.valid_from:yyyy-MM-dd} with no end date."
                : $"A price is already set for this product from {existingPrice.valid_from:yyyy-MM-dd} to {existingPrice.valid_to:yyyy-MM-dd}.";
            
            throw new InvalidOperationException(message);
        }
        
        await _dbContext.Connection.ExecuteAsync(
            "insert into catalog.product_prices (product_id, price, valid_from) values (@productId, @price, @validFrom)", 
            new
            {
                productId = productId.Value,
                price,
                validFrom = validFrom.UtcDateTime
            },
            _dbContext.CurrentTransaction
        );
    }
}