using ContextDrivenDevelopment.Api.Products;
using MassTransit;

namespace ContextDrivenDevelopment.Api.Inventory.EventConsumers;

/// <summary>
/// A consumer responsible for handling the <see cref="ProductCreated"/> event
/// and performing actions related to inventory management.
/// </summary>
public sealed class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly IInventoryRepository _inventory;

    public ProductCreatedConsumer(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var item = InventoryItem.CreateNew(context.Message.ProductSlug);
        await _inventory.UpsertAsync(item, context.CancellationToken);
    }
}
