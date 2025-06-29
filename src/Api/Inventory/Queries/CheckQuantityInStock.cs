namespace ContextDrivenDevelopment.Api.Inventory.Queries;

using Result = Results<Ok<int>, NotFoundResult>;

public static class CheckQuantityInStock
{
    public sealed class QueryHandler
    {
        private readonly IInventoryRepository _inventory;
        
        public QueryHandler(IInventoryRepository inventory)
        {
            _inventory = inventory;
        }

        public async Task<Result> Handle(string productSlug, CancellationToken cancellationToken = default)
        {
            var item = await _inventory.GetAsync(productSlug, cancellationToken);
            
            return item.Match<Result>(
                ok => TypedResults.Ok(ok.QuantityAvailable),
                _ => TypedResults.NotFound()
            );
        }
        
    }
}