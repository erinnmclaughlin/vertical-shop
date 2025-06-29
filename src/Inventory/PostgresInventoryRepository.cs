using Dapper;

namespace VerticalShop.Inventory;

/// <inheritdoc />
internal sealed class PostgresInventoryRepository(IDatabaseContext dbContext) : IInventoryRepository
{
    private readonly IDatabaseContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<InventoryItem?> GetAsync(string productSlug, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _dbContext.Connection.QuerySingleOrDefaultAsync<InventoryItem>(
            """
            select 
                product_slug as "ProductSlug",
                quantity as "QuantityAvailable"
            from inventory.items
            where product_slug = @productSlug
            """,
            new { productSlug }
        );
    }

    /// <inheritdoc />
    public async Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await _dbContext.Connection.ExecuteAsync(
            """
            insert into inventory.items (product_slug, quantity)
            values (@slug, @quantity)
            on conflict (product_slug) do update set quantity = @quantity;
            """, 
            new { slug = item.ProductSlug, quantity = item.QuantityAvailable });
    }
}
