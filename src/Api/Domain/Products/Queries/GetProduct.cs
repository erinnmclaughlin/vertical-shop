using Microsoft.AspNetCore.Http.HttpResults;
using NotFoundResult = Microsoft.AspNetCore.Http.HttpResults.NotFound;

namespace ContextDrivenDevelopment.Api.Domain.Products.Queries;

using Result = Results<Ok<GetProduct.ProductDetail>, NotFoundResult>;

public static class GetProduct
{
    public sealed record ProductDetail
    {
        public required Guid Id { get; init; }
        public required string Slug { get; init; }
        public required string Name { get; init; }
        public required IReadOnlyDictionary<string, string> Attributes { get; init; }
    }

    public sealed class QueryHandler
    {
        private readonly IProductRepository _productRepository;
        
        public QueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        
        public async Task<Result> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _productRepository.GetByIdAsync(id, cancellationToken);
            return MapToHttpResult(result);
        }

        public async Task<Result> GetBySlug(string slug, CancellationToken cancellationToken = default)
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
