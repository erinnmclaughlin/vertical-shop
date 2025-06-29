using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Inventory.Queries;

using Result = Results<Ok<int>, NotFound>;

/// <summary>
/// Provides functionality to check the available quantity of a product in stock.
/// </summary>
public static class CheckQuantityInStock
{
    /// <summary>
    /// Handles the process of querying the inventory for available quantity of a product.
    /// </summary>
    public sealed class QueryHandler(IInventoryRepository inventory)
    {
        private readonly IInventoryRepository _inventory = inventory;

        /// <summary>
        /// Handles the process of retrieving the quantity of a product available in inventory.
        /// </summary>
        /// <param name="productSlug">The unique identifier or slug of the product to check in the inventory.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation if necessary.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is a <c>Result</c> object containing an <c>Ok</c> result with the quantity available if the product is found, or a <c>NotFound</c> result otherwise.
        /// </returns>
        public async Task<Result> Handle(string productSlug, CancellationToken cancellationToken = default)
        {
            var item = await _inventory.GetAsync(productSlug, cancellationToken);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(item.QuantityAvailable);
        }
    }
}
