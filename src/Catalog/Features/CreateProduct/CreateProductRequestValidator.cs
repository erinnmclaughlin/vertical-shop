using FluentValidation;

namespace VerticalShop.Catalog.Features.CreateProduct;

internal sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
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
        return !string.IsNullOrWhiteSpace(value) &&
               !value.StartsWith('-')
               && !value.EndsWith('-')
               && value.All(c => char.IsLetterOrDigit(c) || c == '-');
    }
}
