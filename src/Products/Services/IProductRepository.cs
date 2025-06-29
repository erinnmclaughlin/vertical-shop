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