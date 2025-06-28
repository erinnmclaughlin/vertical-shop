using ContextDrivenDevelopment.Api.Persistence;
using ContextDrivenDevelopment.Api.Persistence.Postgres;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace ContextDrivenDevelopment.Api.Tests;

/// <summary>
/// Provides a fixture for setting up and configuring the test environment for API integration tests.
/// Inherits from <see cref="WebApplicationFactory{TEntryPoint}"/> to manage the creation of a test server.
/// Implements <see cref="IAsyncLifetime"/> to manage asynchronous setup and teardown processes.
/// </summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly NpgsqlDataSource _dataSource;
    
    private DatabaseFixture Database { get; }
    private IUnitOfWork UnitOfWork { get; }

    public ApiFixture(DatabaseFixture database)
    {
        _dataSource = new NpgsqlDataSourceBuilder(database.GetConnectionString()).Build();
        
        Database = database;
        UnitOfWork = new PostgresUnitOfWork(_dataSource);
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<NpgsqlDataSource>()
                .AddSingleton(_dataSource);

            services
                .RemoveAll<IUnitOfWork>()
                .AddSingleton(UnitOfWork);
            
            services
                .ConfigureRunner(rb => rb.WithGlobalConnectionString(Database.GetConnectionString()))
                .BuildServiceProvider()
                .GetRequiredService<IMigrationRunner>()
                .MigrateUp();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await Database.EnsureCreatedAsync();
    }
}
