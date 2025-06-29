using VerticalShop.Api.Messaging;
using VerticalShop.Api.Persistence;

namespace VerticalShop.Api.Products;

using Result = Results<Created, ValidationProblem>;

/// <summary>
/// Provides the implementation for creating a new product within the application.
/// </summary>
public static class CreateProduct
{
    /// <summary>
    /// Represents a request to create a new <see cref="Product"/>.
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
        /// <inheritdoc />
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

        /// <summary>
        /// Handles the asynchronous execution of a product creation command.
        /// </summary>
        /// <param name="command">The <see cref="CreateProduct.Command"/> containing product creation details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of type <see cref="Result"/>.
        /// The result may represent a successful product creation or validation errors.
        /// </returns>
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
            await _outbox.InsertMessage(ProductCreated.FromProduct(product), cancellationToken);
            
            // commit the changes
            await transaction.CommitAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
