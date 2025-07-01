using System.Text.Json;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides the implementation for creating a new product variant.
/// </summary>
public static class CreateProductVariant
{
    /// <summary>
    /// A request to create a new product.
    /// </summary>
    public sealed record RequestBody(
        string Name,
        IReadOnlyDictionary<string, string>? Attributes = null
    );
    
    /// <summary>
    /// A request to create a new product.
    /// </summary>
    public sealed record Command(
        Guid ProductId,
        string Name,
        IReadOnlyDictionary<string, string>? Attributes = null
    );
    
    /// <summary>
    /// A validator for <see cref="CreateProductVariant.Command"/> instances.
    /// </summary>
    public sealed class RequestValidator : AbstractValidator<Command>
    {
        /// <inheritdoc />
        public RequestValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("The product ID must be specified.");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The variant name must be specified.")
                .MaximumLength(200).WithMessage("The variant name must not exceed 200 characters.");
        }
    }

    internal sealed class CommandHandler(
        IDatabaseContext dbContext,
        IValidator<Command> validator)
    {
        private readonly IDatabaseContext _dbContext = dbContext;
        private readonly IValidator<Command> _validator = validator;
        
        public async Task<Results<Created, ValidationProblem>> Handle(Command command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (await _validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }

            var variantId = Guid.CreateVersion7();
            
            await _dbContext.Connection.ExecuteAsync(
                """
                INSERT INTO catalog.product_variants (id, product_id, name, attributes) 
                VALUES (@Id, @ProductId, @Name, @Attributes::jsonb)
                """,
                 new
                 {
                     Id = variantId,
                     command.ProductId,
                     command.Name,
                     Attributes = JsonSerializer.Serialize(command.Attributes)
                 }
            );

            return TypedResults.Created();
        }
    }
}