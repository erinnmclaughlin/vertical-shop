namespace ContextDrivenDevelopment.Api.Products;

using Result = Results<Ok<ProductDto>, NotFoundResult>;

public static class GetProduct
{
    public sealed class QueryHandler
    {
        private readonly IProductRepository _products;
        
        public QueryHandler(IProductRepository products)
        {
            _products = products;
        }

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result which could either be the product details if found, or a not found result.</returns>
        public async Task<Result> GetById(ProductId id, CancellationToken cancellationToken = default)
        {
            var result = await _products.GetByIdAsync(id, cancellationToken);
            return result.Match<Result>(
                p => TypedResults.Ok(ProductDto.FromProduct(p)),
                _ => TypedResults.NotFound()
            );
        }

        /// <summary>
        /// Retrieves a product by its unique slug.
        /// </summary>
        /// <param name="slug">The unique slug of the product to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result which could either be the product details if found, or a not found result.</returns>
        public async Task<Result> GetBySlug(ProductSlug slug, CancellationToken cancellationToken = default)
        {
            var result = await _products.GetBySlugAsync(slug, cancellationToken);
            return result.Match<Result>(
                p => TypedResults.Ok(ProductDto.FromProduct(p)),
                _ => TypedResults.NotFound()
            );
        }
    }
}
