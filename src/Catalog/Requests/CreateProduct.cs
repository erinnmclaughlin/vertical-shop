using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for creating a new product within the application.
/// </summary>
public static class CreateProduct
{
    /// <summary>
    /// Represents a request to create a new <see cref="Product"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed record Command
    {
        /// <summary>
        /// The product slug.
        /// </summary>
        /// <remarks>
        /// Note that this value cannot be changed.
        /// </remarks>
        public required string Slug { get; init; }
        
        /// <summary>
        /// The product name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// A collection of key-value pairs representing additional attributes of the product.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Attributes { get; init; }
    }

    /// <summary>
    /// Request validator for <see cref="Command"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public sealed class CommandValidator : AbstractValidator<Command>
    {
        /// <inheritdoc />
        public CommandValidator(IProductRepository productRepository)
        {
            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(200)
                .CustomAsync(async (slug, context, ct) =>
                {
                    var product = await productRepository.GetBySlugAsync(ProductSlug.Parse(slug), ct);
                    if (product is not null)
                    {
                        context.AddFailure($"A product with the slug '{slug}' already exists.");
                    }
                });
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
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
        /// Handles the execution of the command to create a product asynchronously.
        /// </summary>
        /// <param name="command">The command containing the product details to be created.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result indicating the outcome of the product creation process.
        /// Returns <see cref="Created"/> if successful or <see cref="ValidationProblem"/> if validation errors occur.</returns>
        public async Task<Results<Created, ValidationProblem>> HandleAsync(Command command,
            CancellationToken cancellationToken = default)
        {
            // validate the request
            if (await _validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }

            // build the product entity from the command
            var product = new Product
            {
                Slug = ProductSlug.Parse(command.Slug),
                Name = command.Name,
                Attributes = command.Attributes?.ToDictionary() ?? []
            };
            
            // start a database transaction
            await _databaseContext.BeginTransactionAsync(cancellationToken);

            // persist the product to the database
            await _productRepository.CreateAsync(product, cancellationToken);
            
            // insert an outbox message to notify other services about the product creation
            var message = new ProductCreated(product.Id, product.Slug, product.Name);
            await _databaseContext.InsertOutboxMessageAsync(message, cancellationToken);
            
            // commit the changes
            await _databaseContext.CommitTransactionAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
