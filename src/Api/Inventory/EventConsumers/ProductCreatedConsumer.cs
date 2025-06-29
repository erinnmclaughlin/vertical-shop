using ContextDrivenDevelopment.Api.Products.Events;
using MassTransit;

namespace ContextDrivenDevelopment.Api.Inventory.EventConsumers;

public sealed class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly IInventoryRepository _inventory;

    public ProductCreatedConsumer(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var item = InventoryItem.CreateNew(context.Message.ProductSlug);
        await _inventory.UpsertAsync(item, context.CancellationToken);
    }
}
