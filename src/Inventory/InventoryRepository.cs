using Dapper;

namespace VerticalShop.Inventory;

/// <summary>
/// Provides abstraction for inventory repository operations.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Retrieves the inventory item associated with the specified product slug.
    /// </summary>
    /// <param name="productSlug">The unique slug identifying the product.</param>
    /// <param name="cancellationToken">The cancellation token to observe during the operation.</param>
    /// <returns>A task containing the result, which is either the inventory item or a not found indication.</returns>
    Task<InventoryItem?> GetAsync(string productSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing inventory item if it exists, otherwise inserts a new inventory item.
    /// </summary>
    /// <param name="item">The inventory item to upsert.</param>
    /// <param name="cancellationToken">The cancellation token to observe during the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
internal sealed class InventoryRepository(IDatabaseContext dbContext) : IInventoryRepository
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
            new { productSlug },
            _dbContext.CurrentTransaction
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
            new { slug = item.ProductSlug, quantity = item.QuantityAvailable },
            _dbContext.CurrentTransaction);
    }
}
