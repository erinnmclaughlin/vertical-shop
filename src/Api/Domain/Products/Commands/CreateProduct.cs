using ContextDrivenDevelopment.Api.Domain.Products.Events;
using ContextDrivenDevelopment.Api.Persistence;

namespace ContextDrivenDevelopment.Api.Domain.Products.Commands;

using Result = Results<Created, ValidationProblem>;

public static class CreateProduct
{
    /// <summary>
    /// Represents a command to create a new <see cref="Product"/>.
    /// </summary>
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
    public sealed class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator(IDatabaseContext databaseContext)
        {
            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(200)
                .CustomAsync(async (slug, context, ct) =>
                {
                    var result = await databaseContext.Products.GetBySlugAsync(ProductSlug.Parse(slug), ct);
                    result.Switch(
                        _ => context.AddFailure($"A product with the slug '{slug}' already exists."),
                        _ => { }
                    );
                });
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        }
    }

    /// <summary>
    /// Request handler for <see cref="Command"/>.
    /// </summary>
    public sealed class CommandHandler
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IValidator<Command> _validator;
        
        public CommandHandler(IDatabaseContext databaseContext, IValidator<Command> validator)
        {
            _databaseContext = databaseContext;
            _validator = validator;
        }
        
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken = default)
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
            await using var transaction = _databaseContext.BeginTransaction();

            // persist the product to the database
            await _databaseContext.Products.CreateAsync(product, cancellationToken);
            
            // insert an outbox message to notify other services about the product creation
            await _databaseContext.Outbox.InsertMessage(new ProductCreated(product.Slug), cancellationToken);
            
            // commit the changes
            await transaction.CommitAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
