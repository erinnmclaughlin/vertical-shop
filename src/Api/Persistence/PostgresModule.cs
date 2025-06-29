using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerticalShop.Api.Persistence;

/// <summary>
/// Provides functionality to register and configure services for Postgres database integration.
/// </summary>
public static class PostgresModule
{
    /// <summary>
    /// Configures and registers services for integrating with a Postgres database.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application's services and settings.</param>
    public static void AddPostgres(this WebApplicationBuilder builder)
    {
        builder.AddNpgsqlDataSource("vertical-shop-db");
        builder.Services.TryAddScoped<IDatabaseContext, PostgresDatabaseContext>();
        builder.Services.TryAddTransient<PostgresDatabaseInitializer>();
        builder.Services.AddScoped<IVersionTableMetaData, CustomVersionTableMetaData>();
        builder.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres();
                rb.WithGlobalConnectionString(builder.Configuration.GetConnectionString("vertical-shop-db"));
                rb.ScanIn(typeof(Program).Assembly).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}
