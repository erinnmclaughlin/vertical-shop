using System.Diagnostics.CodeAnalysis;
using MassTransit;

namespace VerticalShop.Inventory.EventConsumers;

/// <summary>
/// A consumer responsible for handling the <see cref="ProductCreated"/> event
/// and performing actions related to inventory management.
/// </summary>
internal sealed class ProductCreatedConsumer(IInventoryRepository inventory) : IConsumer<ProductCreated>
{
    private readonly IInventoryRepository _inventory = inventory;

    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var item = InventoryItem.CreateNew(context.Message.ProductSlug);
        await _inventory.UpsertAsync(item, context.CancellationToken);
    }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed record ProductCreated(string ProductId, string ProductSlug, string ProductName);
