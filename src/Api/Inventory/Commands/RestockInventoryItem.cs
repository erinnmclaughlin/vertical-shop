using Microsoft.AspNetCore.Mvc;

namespace ContextDrivenDevelopment.Api.Inventory.Commands;

using Result = Results<NoContent, NotFoundResult>;

public static class RestockInventoryItem
{
    /// <summary>
    /// Represents the body of the API request for restocking an inventory item.
    /// </summary>
    public sealed record RequestBody(int Quantity);

    /// <summary>
    /// Represents the command to restock an inventory item, including details about the product and quantity.
    /// </summary>
    /// <param name="ProductSlug">The unique slug identifier representing the product in the inventory system.</param>
    /// <param name="Quantity">The quantity of the product to be restocked.</param>
    public sealed record Command(string ProductSlug, int Quantity);

    /// <summary>
    /// Provides validation rules for the <see cref="RestockInventoryItem.Command"/> class.
    /// </summary>
    public sealed class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductSlug)
                .NotEmpty()
                .MaximumLength(200);
            
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Handles the execution of the <see cref="RestockInventoryItem.Command"/>.
    /// </summary>
    public sealed class CommandHandler
    {
        private readonly IInventoryRepository _inventoryRepository;
        
        public CommandHandler(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }
        
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            var getItemResult = await _inventoryRepository.GetAsync(command.ProductSlug, cancellationToken);

            return await getItemResult.Match<Task<Result>>(
                item => Handle(command, item, cancellationToken),
                _ => Task.FromResult<Result>(TypedResults.NotFound())
            );
        }

        private async Task<Result> Handle(Command command, InventoryItem item, CancellationToken cancellationToken = default)
        {
            item.QuantityAvailable += command.Quantity;
            await _inventoryRepository.UpsertAsync(item, cancellationToken);
            return TypedResults.NoContent();
        }
    }
}