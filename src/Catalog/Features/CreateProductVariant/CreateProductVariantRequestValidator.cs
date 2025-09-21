using FluentValidation;

namespace VerticalShop.Catalog.Features.CreateProductVariant;

internal sealed class CreateProductVariantRequestValidator : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("The product ID must be specified.");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The variant name must be specified.")
            .MaximumLength(200).WithMessage("The variant name must not exceed 200 characters.");
    }
}
