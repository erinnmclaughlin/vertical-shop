using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for retrieving product details.
/// </summary>
public static class GetProduct
{
    /// <summary>
    /// Handles requests to get a specific product.
    /// </summary>
    public sealed class QueryHandler(IProductRepository products)
    {
        private readonly IProductRepository _products = products;

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result which could either be the product details if found, or a not found result.</returns>
        public async Task<Results<Ok<ProductDto>, NotFound>> GetById(ProductId id, CancellationToken cancellationToken = default)
        {
            var result = await _products.GetByIdAsync(id, cancellationToken);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(ProductDto.FromProduct(result));
        }

        /// <summary>
        /// Retrieves a product by its unique slug.
        /// </summary>
        /// <param name="slug">The unique slug of the product to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result which could either be the product details if found, or a not found result.</returns>
        public async Task<Results<Ok<ProductDto>, NotFound>> GetBySlug(ProductSlug slug, CancellationToken cancellationToken = default)
        {
            var result = await _products.GetBySlugAsync(slug, cancellationToken);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(ProductDto.FromProduct(result));
        }
    }
}
