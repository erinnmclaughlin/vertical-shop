using System.Diagnostics.CodeAnalysis;

namespace ContextDrivenDevelopment.Api.Products;

public static class ListProducts
{
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 20;

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public sealed record Query
    {
        public int? Offset { get; init; } = DefaultOffset;
        public int? Limit { get; init; } = DefaultLimit;
    }
    
    public sealed class QueryHandler
    {
        private readonly IProductRepository _productRepository;

        public QueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Ok<List<ProductDto>>> Handle(Query query, CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.ListAsync(
                query.Offset ?? DefaultOffset,
                query.Limit ?? DefaultLimit,
                cancellationToken
            );

            return TypedResults.Ok(
                products.Select(p => new ProductDto
                {
                    Id = p.Id.Value,
                    Name = p.Name,
                    Slug = p.Slug.Value,
                    Attributes = p.Attributes
                })
                .ToList()
            );
        }
    }
}