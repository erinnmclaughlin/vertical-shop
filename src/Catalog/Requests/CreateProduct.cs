using System.Diagnostics.CodeAnalysis;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Catalog;

using Result = Results<Created, ValidationProblem, Conflict>;

/// <summary>
/// Provides the implementation for creating a new product.
/// </summary>
public static class CreateProduct
{
    /// <summary>
    /// A request to create a new product.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed record Command(string Slug, string Name) : IRequest<Result>;

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
                .Must(IsValidSlug).WithMessage("The product slug must be a valid slug.");
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        }
        
        private static bool IsValidSlug(string value)
        {
            // Slug should only contain lowercase letters, numbers, and hyphens
            // It should not start or end with a hyphen
            return !string.IsNullOrWhiteSpace(value) &&
                   !value.StartsWith('-') 
                   && !value.EndsWith('-') 
                   && value.All(c => char.IsLetterOrDigit(c) || c == '-');
        }
    }
    
    internal sealed class CommandHandler(
        NpgsqlDataSource dataSource, 
        IValidator<Command> validator
    ) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken = default)
        {
            if (await validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }
            
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            var productId = Guid.CreateVersion7();
            
            try
            {
                await connection.ExecuteAsync(
                    "insert into catalog.products (id, name, slug) values (@Id, @Name, @Slug)", 
                    new { Id = productId, command.Name, command.Slug },
                    transaction
                );
            }
            catch (PostgresException ex) when (ex.IsUniqueConstraintViolationOnColumn("slug"))
            {
                return TypedResults.Conflict();
            }

            var message = new ProductCreated(productId, command.Slug, command.Name);
            await connection.InsertOutboxMessageAsync(message, transaction, cancellationToken); 
            
            await transaction.CommitAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
