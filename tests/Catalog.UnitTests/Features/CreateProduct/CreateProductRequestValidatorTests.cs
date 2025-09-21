using FluentValidation.TestHelper;
using VerticalShop.Catalog.Features.CreateProduct;

namespace VerticalShop.Catalog.UnitTests.Features.CreateProduct;

public sealed class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Validator_should_have_error_when_name_is_empty()
    {
        var validationResult = _validator.TestValidate(EmptyRequest);
        validationResult.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_should_have_error_when_name_is_too_long()
    {
        var validationResult = _validator.TestValidate(EmptyRequest with
        {
            Name = new string('a', 201)
        });

        validationResult.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_should_not_have_error_when_name_is_valid()
    {
        var validationResult = _validator.TestValidate(EmptyRequest with
        {
            Name = "Valid Name"
        });

        validationResult.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_should_contain_error_when_slug_is_empty()
    {
        var validationResult = _validator.TestValidate(EmptyRequest);
        validationResult.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("-slug")]
    [InlineData("slug-")]
    [InlineData("-slug-")]
    public void Validator_should_have_error_when_dash_is_in_invalid_location(string slug)
    {
        var validationResult = _validator.TestValidate(EmptyRequest with
        {
            Slug = slug
        });

        validationResult.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validator_should_not_have_error_when_slug_is_valid()
    {
        var validationResult = _validator.TestValidate(EmptyRequest with
        {
            Slug = "valid-slug"
        });

        validationResult.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    private static CreateProductRequest EmptyRequest => new()
    {
        Name = string.Empty,
        Slug = string.Empty
    };
}
