using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Persistence;
using ContextDrivenDevelopment.Api.Products.Events;

namespace ContextDrivenDevelopment.Api.Products.Commands;

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
        public CommandValidator(IProductRepository productRepository)
        {
            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(200)
                .CustomAsync(async (slug, context, ct) =>
                {
                    var result = await productRepository.GetBySlugAsync(ProductSlug.Parse(slug), ct);
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
    public sealed class CommandHandler(
        IDatabaseContext databaseContext,
        IOutbox outbox,
        IProductRepository productRepository, 
        IValidator<Command> validator)
    {
        private readonly IDatabaseContext _databaseContext = databaseContext;
        private readonly IOutbox _outbox = outbox;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IValidator<Command> _validator = validator;
        
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
            await _productRepository.CreateAsync(product, cancellationToken);
            
            // insert an outbox message to notify other services about the product creation
            await _outbox.InsertMessage(new ProductCreated(product.Slug), cancellationToken);
            
            // commit the changes
            await transaction.CommitAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
