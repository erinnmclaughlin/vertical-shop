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
    public async Task<OneOf<InventoryItem, NotFound>> GetAsync(string productSlug, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var item = await _connection.QuerySingleOrDefaultAsync<InventoryItem>(
            """
            select 
                product_slug as "ProductSlug",
                quantity as "QuantityAvailable"
            from inventory.items
            where product_slug = @productSlug
            """,
            new { productSlug }
        );
        
        return item is null ? new NotFound() : item;
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
