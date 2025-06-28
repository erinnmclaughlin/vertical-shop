using ContextDrivenDevelopment.Api.Persistence.Postgres;
using Dapper;

namespace ContextDrivenDevelopment.Api.Domain.Products.Persistence;

/// <inheritdoc />
internal sealed class PostgresProductRepository : IProductRepository
{
    private readonly PostgresUnitOfWork _unitOfWork;

    public PostgresProductRepository(PostgresUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO products.products (id, name, slug)
            VALUES (@id, @name, @slug)
            """;
        
        await _unitOfWork.ExecuteAsync(sql, product, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<OneOf<Product, NotFound>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM products.products WHERE id = @id";
        var product = await _unitOfWork.QuerySingleOrDefaultAsync<Product>(sql, new { id }, cancellationToken);
        return product is null ? new NotFound() : product;
    }
    
    /// <inheritdoc />
    public async Task<OneOf<Product, NotFound>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM products.products WHERE slug = @slug";
        var product = await _unitOfWork.QuerySingleOrDefaultAsync<Product>(sql, new { slug }, cancellationToken);
        return product is null ? new NotFound() : product;
    }
}
