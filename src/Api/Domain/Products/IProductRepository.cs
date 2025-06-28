namespace ContextDrivenDevelopment.Api.Domain.Products;

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
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing either the product if found or a NotFound result.</returns>
    Task<OneOf<Product, NotFound>> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product by its slug.
    /// </summary>
    /// <param name="slug">The unique slug of the product to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing either the product if found or a NotFound result.</returns>
    Task<OneOf<Product, NotFound>> GetBySlugAsync(ProductSlug slug, CancellationToken cancellationToken = default);
}