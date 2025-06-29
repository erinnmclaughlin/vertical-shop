using System.Reflection;
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
    /// Configures and registers the necessary services for PostgreSQL database integration.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance to configure.</param>
    /// <param name="assemblies">An array of assemblies to scan for FluentMigrator migrations.</param>
    public static void AddPostgres(this WebApplicationBuilder builder, Assembly[] assemblies)
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
                rb.ScanIn(assemblies).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}
