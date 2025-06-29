using AutoBogus;
using ContextDrivenDevelopment.Api.Persistence;
using ContextDrivenDevelopment.Api.Products;
using FluentValidation.TestHelper;
using NSubstitute;
using OneOf.Types;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands.CreateProduct;

using Command = Api.Products.Commands.CreateProduct.Command;
using CommandValidator = Api.Products.Commands.CreateProduct.CommandValidator;

public sealed class CreateProductValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnValidationResultWithNoErrors_WhenRequestIsValid()
    {
        var command = new Command
        {
            Slug = "test-product",
            Name = "Test Product"
        };
        
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySlugAsync(Arg.Any<ProductSlug>(), Arg.Any<CancellationToken>()).Returns(new NotFound());
        
        var validator = BuildTestSubject(productRepository);
        var validationResult = await validator.TestValidateAsync(command, null, TestContext.Current.CancellationToken);
        
        validationResult.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task ValidateAsync_ShouldReturnErrorForSlug_WhenSlugIsAlreadyTaken()
    {
        var command = new Command
        {
            Slug = "test-product",
            Name = "Test Product"
        };
        
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySlugAsync(Arg.Any<ProductSlug>(), Arg.Any<CancellationToken>()).Returns(AutoFaker.Generate<Product>());
        
        var validator = BuildTestSubject(productRepository);
        var validationResult = await validator.TestValidateAsync(command, null, TestContext.Current.CancellationToken);
        
        validationResult.ShouldHaveValidationErrorFor(x => x.Slug);
    }
    
    [Fact]
    public async Task ValidateAsync_ShouldReturnErrorForSlug_WhenEmpty()
    {
        var command = new Command
        {
            Slug = "",
            Name = "Test Product"
        };
        
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySlugAsync(Arg.Any<ProductSlug>(), Arg.Any<CancellationToken>()).Returns(new NotFound());
        
        var validator = BuildTestSubject(productRepository);
        var validationResult = await validator.TestValidateAsync(command, null, TestContext.Current.CancellationToken);
        
        validationResult.ShouldHaveValidationErrorFor(x => x.Slug);
    }
    
    [Fact]
    public async Task ValidateAsync_ShouldReturnErrorForSlug_WhenTooLong()
    {
        var command = new Command
        {
            Slug = new string('x', 201),
            Name = "Test Product"
        };
        
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySlugAsync(Arg.Any<ProductSlug>(), Arg.Any<CancellationToken>()).Returns(new NotFound());
        
        var validator = BuildTestSubject(productRepository);
        var validationResult = await validator.TestValidateAsync(command, null, TestContext.Current.CancellationToken);
        
        validationResult.ShouldHaveValidationErrorFor(x => x.Slug);
    }
    
    [Fact]
    public async Task ValidateAsync_ShouldReturnErrorForName_WhenEmpty()
    {
        var command = new Command
        {
            Slug = "test-product",
            Name = ""
        };
        
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySlugAsync(Arg.Any<ProductSlug>(), Arg.Any<CancellationToken>()).Returns(new NotFound());
        
        var validator = BuildTestSubject(productRepository);
        var validationResult = await validator.TestValidateAsync(command, null, TestContext.Current.CancellationToken);
        
        validationResult.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public async Task ValidateAsync_ShouldReturnErrorForName_WhenTooLong()
    {
        var command = new Command
        {
            Slug = "test-product",
            Name = new string('x', 201)
        };
        
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySlugAsync(Arg.Any<ProductSlug>(), Arg.Any<CancellationToken>()).Returns(new NotFound());
        
        var validator = BuildTestSubject(productRepository);
        var validationResult = await validator.TestValidateAsync(command, null, TestContext.Current.CancellationToken);
        
        validationResult.ShouldHaveValidationErrorFor(x => x.Name);
    }

    private static CommandValidator BuildTestSubject(IProductRepository productRepository)
    {
        var unitOfWork = Substitute.For<IDatabaseContext>();
        unitOfWork.Products.Returns(productRepository);
        return new CommandValidator(unitOfWork);   
    }
}
