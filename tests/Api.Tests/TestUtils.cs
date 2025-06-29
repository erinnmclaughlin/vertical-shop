using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace VerticalShop.Api.Tests;

public static class TestUtils
{
    public static IValidator<T> CreateMockValidatorFor<T>(T request, bool isValid = true)
    {
        var validator = Substitute.For<IValidator<T>>();

        var validationResult = new ValidationResult();

        if (!isValid)
        {
            validationResult.Errors.Add(new ValidationFailure("Test", "Test Error"));
        }
        
        validator.Validate(request).Returns(validationResult);
        validator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(validationResult);
        
        return validator;
    }
}
