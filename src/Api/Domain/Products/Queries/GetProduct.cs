using Microsoft.AspNetCore.Http.HttpResults;
using NotFoundResult = Microsoft.AspNetCore.Http.HttpResults.NotFound;

namespace ContextDrivenDevelopment.Api.Domain.Products.Queries;

using Result = Results<Ok<GetProduct.ProductDetail>, NotFoundResult>;

public static class GetProduct
{
    /// <summary>
    /// Represents the details of a product.
    /// </summary>
    public sealed record ProductDetail
    {
        /// <summary>
        /// Gets the unique identifier of the product.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the unique URL-friendly identifier for a product.
        /// </summary>
        public required string Slug { get; init; }

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the attributes associated with the product as key-value pairs.
        /// </summary>
        public required IReadOnlyDictionary<string, string> Attributes { get; init; }
    }

    public sealed class QueryHandler
    {
        private readonly IProductRepository _productRepository;
        
        public QueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result which could either be the product details if found, or a not found result.</returns>
        public async Task<Result> GetById(ProductId id, CancellationToken cancellationToken = default)
        {
            var result = await _productRepository.GetByIdAsync(id, cancellationToken);
            return MapToHttpResult(result);
        }

        /// <summary>
        /// Retrieves a product by its unique slug.
        /// </summary>
        /// <param name="slug">The unique slug of the product to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result which could either be the product details if found, or a not found result.</returns>
        public async Task<Result> GetBySlug(ProductSlug slug, CancellationToken cancellationToken = default)
        {
            var result = await _productRepository.GetBySlugAsync(slug, cancellationToken);
            return MapToHttpResult(result);
        }
        
        private static Result MapToHttpResult(OneOf<Product, OneOf.Types.NotFound> result) => result.Match<Result>(
            p => TypedResults.Ok(new ProductDetail
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = p.Name,
                Attributes = p.Attributes
            }),
            _ => TypedResults.NotFound()
        );
    }
}
