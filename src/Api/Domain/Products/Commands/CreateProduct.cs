using ContextDrivenDevelopment.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

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
        public required string Slug { get; init; }
        
        /// <summary>
        /// The product name.
        /// </summary>
        public required string Name { get; init; }
    }

    public sealed class CommandValidator : AbstractValidator<Command>
    {
        private readonly IProductRepository _productRepository;

        public CommandValidator(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(200)
                .CustomAsync(async (slug, context, ct) =>
                {
                    var result = await _productRepository.GetBySlugAsync(slug, ct);
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

    public sealed class CommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Command> _validator;
        
        public CommandHandler(IUnitOfWork unitOfWork, IValidator<Command> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }
        
        public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            if (await _validator.ValidateAsync(command, cancellationToken) is { IsValid: false } error)
            {
                return TypedResults.ValidationProblem(error.ToDictionary());
            }

            var product = new Product
            {
                Slug = command.Slug,
                Name = command.Name
            };

            await _unitOfWork.Products.CreateAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            return TypedResults.Created();
        }
    }
}
