using ContextDrivenDevelopment.Api.Domain.Products.Events;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Persistence;

namespace ContextDrivenDevelopment.Api.Domain.Inventory.EventConsumers;

public sealed class ProductCreatedConsumer
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductCreatedConsumer(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ConsumeAsync(ProductCreated @event)
    {
        var item = new InventoryItem
        {
            ProductSlug = @event.ProductSlug,
            QuantityAvailable = 0
        };

        await _unitOfWork.Inventory.UpsertAsync(item);
        await _unitOfWork.CommitAsync();
    }
}
