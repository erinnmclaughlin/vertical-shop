using ContextDrivenDevelopment.Api.Domain.Products.Events;
using ContextDrivenDevelopment.Api.Persistence;
using MassTransit;

namespace ContextDrivenDevelopment.Api.Domain.Inventory.EventConsumers;

public sealed class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductCreatedConsumer(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var item = new InventoryItem
        {
            ProductSlug = context.Message.ProductSlug,
            QuantityAvailable = 0
        };

        await _unitOfWork.Inventory.UpsertAsync(item);
    }
}
