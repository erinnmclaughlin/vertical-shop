using ContextDrivenDevelopment.Api.Domain.Products.Events;
using ContextDrivenDevelopment.Api.Persistence;
using MassTransit;

namespace ContextDrivenDevelopment.Api.Domain.Inventory.EventConsumers;

public sealed class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly IInventoryRepository _inventory;

    public ProductCreatedConsumer(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var item = new InventoryItem
        {
            ProductSlug = context.Message.ProductSlug,
            QuantityAvailable = 0
        };

        await _inventory.UpsertAsync(item, context.CancellationToken);
    }
}
