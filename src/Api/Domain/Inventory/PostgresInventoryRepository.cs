using ContextDrivenDevelopment.Api.Persistence.Postgres;

namespace ContextDrivenDevelopment.Api.Domain.Inventory;

/// <inheritdoc />
internal sealed class PostgresInventoryRepository : IInventoryRepository
{
    private NpgsqlConnection Connection { get; }

    public PostgresInventoryRepository(NpgsqlDataSource dataSource)
    {
        Connection = dataSource.OpenConnection();
    }
    
    public PostgresInventoryRepository(PostgresUnitOfWork unitOfWork)
    {
        Connection = unitOfWork.Connection;
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
        
        cancellationToken.ThrowIfCancellationRequested();
        await Connection.ExecuteAsync(sql, parameters);
    }
}