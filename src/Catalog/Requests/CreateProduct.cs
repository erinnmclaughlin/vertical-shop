using System.Diagnostics.CodeAnalysis;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Catalog;

/// <summary></summary>
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
        IDatabaseContext dbContext,
        IValidator<Command> validator
    )
    {
        public async Task<Results<Created, ValidationProblem, Conflict>> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            if (await validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }
            
            await dbContext.BeginTransactionAsync(cancellationToken);

            var productId = Guid.CreateVersion7();
            
            try
            {
                await dbContext.Connection.ExecuteAsync(
                    "insert into catalog.products (id, name, slug) values (@Id, @Name, @Slug)", 
                    new { Id = productId, command.Name, command.Slug },
                    dbContext.CurrentTransaction
                );
            }
            catch (PostgresException ex) when (ex.IsUniqueConstraintViolationOnColumn("slug"))
            {
                return TypedResults.Conflict();
            }

            var message = new ProductCreated(productId.ToString(), command.Slug, command.Name);
            await dbContext.InsertOutboxMessageAsync(message, cancellationToken); 
            
            await dbContext.CommitTransactionAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
