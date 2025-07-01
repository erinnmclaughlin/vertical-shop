using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MassTransit.SqlTransport;
using MassTransit.SqlTransport.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerticalShop.Api;

internal static class Extensions
{
    /// <summary>
    /// Configures and registers the necessary services for PostgreSQL database integration.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance to configure.</param>
    /// <param name="assemblies">An array of assemblies to scan for FluentMigrator migrations.</param>
    public static void AddDatabaseInitialization(this WebApplicationBuilder builder, Assembly[] assemblies)
    {
        // this is used to create the database tables needed for MassTransit SQL transport:
        builder.Services.AddTransient<ISqlTransportDatabaseMigrator, PostgresDatabaseMigrator>();
        
        // this is used to create migration history table used by FluentMigrator:
        builder.Services.AddScoped<IVersionTableMetaData, CustomVersionTableMetaData>();
        
        // this is the encapsulation of the database initialization logic:
        builder.Services.TryAddTransient<DatabaseInitializer>();
        
        // this sets up the FluentMigrator Postgres runner to apply migrations (used by the DatabaseInitializer):
        builder.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres();
                rb.WithGlobalConnectionString(builder.Configuration.GetConnectionString("vertical-shop-db"));
                rb.ScanIn([typeof(Program).Assembly, ..assemblies]).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}
