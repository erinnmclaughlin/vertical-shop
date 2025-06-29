namespace ContextDrivenDevelopment.Api.Inventory;

/// <summary>
/// Provides abstraction for inventory repository operations.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Updates an existing inventory item if it exists, otherwise inserts a new inventory item.
    /// </summary>
    /// <param name="item">The inventory item to upsert.</param>
    /// <param name="cancellationToken">The cancellation token to observe during the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken = default);
}