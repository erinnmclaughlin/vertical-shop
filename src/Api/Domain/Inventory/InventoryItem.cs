namespace ContextDrivenDevelopment.Api.Domain.Inventory;

public sealed class InventoryItem
{
    /// <summary>
    /// The unique identifier for the product in a URL-friendly format.
    /// </summary>
    public required string ProductSlug { get; init; }

    /// <summary>
    /// The current quantity of the inventory item available for sale.
    /// </summary>
    public int QuantityAvailable { get; set; }
}
