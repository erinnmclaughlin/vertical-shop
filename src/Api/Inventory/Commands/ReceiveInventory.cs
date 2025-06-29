namespace ContextDrivenDevelopment.Api.Inventory.Commands;

using Result = Results<NoContent, NotFoundResult>;

public static class ReceiveInventory
{
    public sealed record Command
    {
        /// <summary>
        /// The unique identifier, typically in a slug format, representing the product in the inventory system.
        /// </summary>
        public required string ProductSlug { get; init; }

        /// <summary>
        ///The quantity of the product that has been received and is to be added to the inventory.
        /// </summary>
        public required int QuantityReceived { get; init; }
    }

    /// <summary>
    /// Provides validation rules for the <see cref="ReceiveInventory.Command"/> class.
    /// </summary>
    public sealed class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductSlug)
                .NotEmpty()
                .MaximumLength(200);
            
            RuleFor(x => x.QuantityReceived)
                .GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Handles the execution of the <see cref="ReceiveInventory.Command"/>.
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
            item.QuantityAvailable += command.QuantityReceived;
            await _inventoryRepository.UpsertAsync(item, cancellationToken);
            return TypedResults.NoContent();
        }
    }
}