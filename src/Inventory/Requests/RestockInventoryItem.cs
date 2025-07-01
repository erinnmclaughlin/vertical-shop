using System.Diagnostics.CodeAnalysis;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace VerticalShop.Inventory;

using Result = Results<NoContent, NotFound>;

/// <summary>
/// Contains functionality for restocking inventory items, including request models,
/// validation logic, and command handling.
/// </summary>
public static class RestockInventoryItem
{
    /// <summary>
    /// Represents the body of the API request for restocking an inventory item.
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed record RequestBody(int Quantity);

    /// <summary>
    /// Represents the command to restock an inventory item, including details about the product and quantity.
    /// </summary>
    /// <param name="ProductSlug">The unique slug identifier representing the product in the inventory system.</param>
    /// <param name="Quantity">The quantity of the product to be restocked.</param>
    public sealed record Command(string ProductSlug, int Quantity) : IRequest<Result>;

    /// <summary>
    /// Provides validation rules for the <see cref="RestockInventoryItem.Command"/> class.
    /// </summary>
    public sealed class CommandValidator : AbstractValidator<Command>
    {
        /// <inheritdoc />
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
    public sealed class CommandHandler(NpgsqlDataSource dataSource) : IRequestHandler<Command, Result>
    {
        /// <summary>
        /// Handles the execution of the given command by performing inventory operations.
        /// </summary>
        /// <param name="command">The command containing the details of the inventory operation to be performed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result indicating the outcome of the command, either success with no content or a not-found result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken = default)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            
            var affectedRowCount = await connection.ExecuteAsync(
                """
                update inventory.items
                set quantity = items.quantity + @quantityToAdd
                where product_slug = @productSlug
                """, 
                new { productSlug = command.ProductSlug, quantityToAdd = command.Quantity }
            );
            
            return affectedRowCount == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
        }
    }
}
