using ContextDrivenDevelopment.Api.Persistence.Postgres;

namespace ContextDrivenDevelopment.Api.Domain.Inventory;

/// <inheritdoc />
internal sealed class PostgresInventoryRepository : IInventoryRepository
{
    private readonly PostgresUnitOfWork _unitOfWork;
    
    public PostgresInventoryRepository(PostgresUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           insert into inventory.items (product_slug, quantity)
                            values (@productSlug, @quantity)
                            on conflict (product_slug) do update set quantity = @quantity;
                           """;

        var parameters = new { productSlug = item.ProductSlug, quantity = item.QuantityAvailable };
        await _unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}