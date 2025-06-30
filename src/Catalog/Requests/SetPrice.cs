using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog;

using Result = Results<Ok, NotFound, ValidationProblem, Conflict<string>>;

/// <summary>
/// Provides the implementation for setting the price of an existing product within the application.
/// </summary>
public static class SetPrice
{
    /// <summary>
    /// Represents a request to set the price for an existing <see cref="Product"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed record Command
    {
        /// <summary>
        /// The product identifier (ID or slug).
        /// </summary>
        public required string Identifier { get; init; }
        
        /// <summary>
        /// The type of identifier being used.
        /// </summary>
        public string IdentifierType { get; init; } = "id";
        
        /// <summary>
        /// The new price for the product.
        /// </summary>
        public required decimal Price { get; init; }
        
        /// <summary>
        /// The date from which this price becomes valid.
        /// </summary>
        public DateTimeOffset? ValidFrom { get; init; }
    }

    /// <summary>
    /// Request validator for <see cref="Command"/>.
    /// </summary>
    public sealed class CommandValidator : AbstractValidator<Command>
    {
        /// <inheritdoc />
        public CommandValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty()
                .WithMessage("Product identifier is required.");
            
            RuleFor(x => x.IdentifierType)
                .Must(x => x is "id" or "slug")
                .WithMessage("Identifier type must be either 'id' or 'slug'.");
            
            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero.");
            
            RuleFor(x => x.ValidFrom)
                .GreaterThanOrEqualTo(DateTimeOffset.UtcNow.Date)
                .When(x => x.ValidFrom.HasValue)
                .WithMessage("Valid from date must be today or in the future.");
        }
    }

    /// <summary>
    /// Request handler for <see cref="Command"/>.
    /// </summary>
    public sealed class CommandHandler(
        IDatabaseContext databaseContext,
        IProductRepository productRepository, 
        IValidator<Command> validator)
    {
        private readonly IDatabaseContext _databaseContext = databaseContext;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IValidator<Command> _validator = validator;

        /// <summary>
        /// Handles the execution of the command to set a product price asynchronously.
        /// </summary>
        /// <param name="command">The command containing the product identifier and new price.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result indicating the outcome of the price setting process.
        /// Returns <see cref="Ok"/> if successful, <see cref="NotFound"/> if product not found, 
        /// or <see cref="ValidationProblem"/> if validation errors occur.</returns>
        public async Task<Result> HandleAsync(Command command,
            CancellationToken cancellationToken = default)
        {
            // validate the request
            if (await _validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }

            // get the product to ensure it exists
            var product = command.IdentifierType is "slug" 
                ? await _productRepository.GetBySlugAsync(ProductSlug.Parse(command.Identifier), cancellationToken)
                : await _productRepository.GetByIdAsync(ProductId.Parse(command.Identifier), cancellationToken);

            if (product is null)
            {
                return TypedResults.NotFound();
            }

            try
            {
                // start a database transaction
                await _databaseContext.BeginTransactionAsync(cancellationToken);

                // set the price
                await _productRepository.UpdatePriceAsync(
                    product.Id, 
                    command.Price, 
                    command.ValidFrom ?? DateTimeOffset.UtcNow, 
                    cancellationToken);
                
                // commit the changes
                await _databaseContext.CommitTransactionAsync(cancellationToken);
                
                return TypedResults.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        }
    }
} 