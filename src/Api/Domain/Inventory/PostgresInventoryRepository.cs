using ContextDrivenDevelopment.Api.Persistence.Postgres;

namespace ContextDrivenDevelopment.Api.Domain.Inventory;

/// <inheritdoc />
internal sealed class PostgresInventoryRepository : IInventoryRepository
{
    private readonly NpgsqlConnection _connection;

    public PostgresInventoryRepository(NpgsqlConnection connection)
    {
        _connection = connection;
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
        await _connection.ExecuteAsync(sql, parameters);
    }
}