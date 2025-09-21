using System.Text.Json;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace VerticalShop.Catalog;

using Result = Results<Created, ValidationProblem>;

/// <summary>
/// Provides the implementation for creating a new product variant.
/// </summary>
public static class CreateProductVariant
{
    /// <summary>
    /// Represents a request to create a new product variant.
    /// </summary>
    /// <param name="Name">The name of the product variant</param>
    /// <param name="Attributes">Attributes associated with the product variant</param>
    public sealed record RequestBody(
        string Name,
        IReadOnlyDictionary<string, string>? Attributes = null
    )
    {
        /// <summary>
        /// Creates a new <see cref="Command"/> instance from the request body,
        /// </summary>
        public Command ToCommand(Guid productId) => new(productId, Name, Attributes);
    }

    /// <summary>
    /// Represents a request to create a new product variant.
    /// </summary>
    /// <param name="ProductId">The product ID</param>
    /// <param name="Name">The name of the product variant</param>
    /// <param name="Attributes">Attributes associated with the product variant</param>
    public sealed record Command(
        Guid ProductId,
        string Name,
        IReadOnlyDictionary<string, string>? Attributes = null
    ) : IRequest<Result>;
    
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
        NpgsqlDataSource dataSource,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            if (await validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }
            
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            await connection.ExecuteAsync(
                """
                INSERT INTO catalog.product_variants (product_id, name, attributes) 
                VALUES (@ProductId, @Name, @Attributes::jsonb)
                """,
                 new
                 {
                     command.ProductId,
                     command.Name,
                     Attributes = JsonSerializer.Serialize(command.Attributes)
                 }
            );

            return TypedResults.Created();
        }
    }
}