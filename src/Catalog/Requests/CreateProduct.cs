using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for creating a new product within the application.
/// </summary>
public static class CreateProduct
{
    /// <summary>
    /// A request to create a new product.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed record Command(string Slug, string Name);

    /// <summary>
    /// A validator for <see cref="CreateProduct.Command"/> instances.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public sealed class RequestValidator : AbstractValidator<Command>
    {
        /// <inheritdoc />
        public RequestValidator()
        {
            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("A unique product slug must be specified.")
                .MaximumLength(200).WithMessage("The product slug must not exceed 200 characters.")
                .Must(slug => ProductSlug.TryParse(slug, out _)).WithMessage("The product slug must be a valid slug.");
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        }
    }

    internal sealed class CommandHandler(
        IDatabaseContext databaseContext,
        IProductRepository productRepository, 
        IValidator<Command> validator)
    {
        private readonly IDatabaseContext _databaseContext = databaseContext;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IValidator<Command> _validator = validator;

        public async Task<Results<Created, ValidationProblem, Conflict>> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            if (await _validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }
            
            await _databaseContext.BeginTransactionAsync(cancellationToken);

            var product = new Product
            {
                Slug = ProductSlug.Parse(command.Slug),
                Name = command.Name
            };
            
            try
            {
                await _productRepository.CreateAsync(product, cancellationToken);
            }
            catch (PostgresException ex) when (ex.IsUniqueConstraintViolationOnColumn("slug"))
            {
                return TypedResults.Conflict();
            }
            
            var message = new ProductCreated(product.Id.Value.ToString(), command.Slug, command.Name);
            await _databaseContext.InsertOutboxMessageAsync(message, cancellationToken);
            
            await _databaseContext.CommitTransactionAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
