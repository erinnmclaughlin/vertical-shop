namespace VerticalShop.Api.Inventory;

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

    /// <summary>
    /// Creates a new instance of the InventoryItem class with the specified product slug.
    /// </summary>
    /// <param name="productSlug">The unique slug identifying the product.</param>
    /// <returns>A new instance of InventoryItem with the specified product slug.</returns>
    public static InventoryItem CreateNew(string productSlug) => new()
    {
        ProductSlug = productSlug
    };
}
