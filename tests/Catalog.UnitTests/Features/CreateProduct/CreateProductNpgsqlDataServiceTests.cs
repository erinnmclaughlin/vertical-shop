using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VerticalShop.Catalog.Features.CreateProduct;

namespace VerticalShop.Catalog.UnitTests.Features.CreateProduct;

public sealed class CreateProductNpgsqlDataServiceTests
{
    [Fact]
    public async Task CreateProduct_PerformsDatabaseOperationsInExpectedOrder_GivenSuccessfulInsert()
    {
        var dataStore = Substitute.For<INpgsqlDataStore>();

        var service = new CreateProductNpgsqlDataService(dataStore);
        await service.CreateProduct(Guid.NewGuid(), "test-slug", "Test Product", TestContext.Current.CancellationToken);

        Received.InOrder(() =>
        {
            dataStore.BeginTransactionAsync(TestContext.Current.CancellationToken);
            dataStore.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), TestContext.Current.CancellationToken);
            dataStore.InsertOutboxMessageAsync(Arg.Any<IntegrationEvents.Products.ProductCreated>(), TestContext.Current.CancellationToken);
            dataStore.CommitTransactionAsync(TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task CreateProduct_DoesNotInsertOutboxMessageOrCommitTransaction_GivenUniqueConstraintViolationException()
    {
        var dataStore = Substitute.For<INpgsqlDataStore>();

        dataStore
            .ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), TestContext.Current.CancellationToken)
            .Throws(new PostgresException(
                messageText: "", 
                severity: "",
                invariantSeverity: "", 
                sqlState: PostgresErrorCodes.UniqueViolation,
                columnName: "slug"));

        var service = new CreateProductNpgsqlDataService(dataStore);
        await service.CreateProduct(Guid.NewGuid(), "test-slug", "Test Product", TestContext.Current.CancellationToken);

        await dataStore.DidNotReceive().InsertOutboxMessageAsync(Arg.Any<IntegrationEvents.Products.ProductCreated>(), TestContext.Current.CancellationToken);
        await dataStore.DidNotReceive().CommitTransactionAsync(TestContext.Current.CancellationToken);
    }
}
