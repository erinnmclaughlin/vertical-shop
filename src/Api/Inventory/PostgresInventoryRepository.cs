namespace ContextDrivenDevelopment.Api.Inventory;

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
        cancellationToken.ThrowIfCancellationRequested();
        
        await _connection.ExecuteAsync(
            """
            insert into inventory.items (product_slug, quantity)
            values (@slug, @quantity)
            on conflict (product_slug) do update set quantity = @quantity;
            """, 
            new { slug = item.ProductSlug, quantity = item.QuantityAvailable });
    }
}
