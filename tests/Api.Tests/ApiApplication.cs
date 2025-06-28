using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ContextDrivenDevelopment.Api.Tests;

public sealed class ApiApplication : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<NpgsqlDataSource>()
                .AddSingleton(new NpgsqlDataSourceBuilder(_postgres.GetConnectionString()).Build());
            
            services
                .ConfigureRunner(rb => rb.WithGlobalConnectionString(_postgres.GetConnectionString()))
                .BuildServiceProvider()
                .GetRequiredService<IMigrationRunner>()
                .MigrateUp();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }
}
