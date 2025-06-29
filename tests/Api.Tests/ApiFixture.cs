using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using VerticalShop.Api.Persistence;

namespace VerticalShop.Api.Tests;

/// <summary>
/// Provides a fixture for setting up and configuring the test environment for API integration tests.
/// Inherits from <see cref="WebApplicationFactory{TEntryPoint}"/> to manage the creation of a test server.
/// Implements <see cref="IAsyncLifetime"/> to manage asynchronous setup and teardown processes.
/// </summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DatabaseFixture Database { get; }

    public ApiFixture(DatabaseFixture database)
    {
        Database = database;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<NpgsqlDataSource>()
                .AddSingleton(new NpgsqlDataSourceBuilder(Database.GetConnectionString()).Build());
                
            services
                .RemoveAll<PostgresDatabaseInitializer>();
            
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
