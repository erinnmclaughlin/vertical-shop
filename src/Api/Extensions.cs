using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MassTransit;
using MassTransit.SqlTransport;
using MassTransit.SqlTransport.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerticalShop.Api;

internal static class Extensions
{
    /// <summary>
    /// Configures messaging services for the application, including MassTransit setup and
    /// the configuration of database options for outbox processing.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="WebApplicationBuilder"/> instance used to configure application services.
    /// </param>
    /// <param name="assemblies">
    /// An array of assemblies to scan for consumers, sagas, and other MassTransit components.
    /// </param>
    public static void AddMessaging(this WebApplicationBuilder builder, Assembly[] assemblies)
    {
        builder.ConfigureMassTransitDatabaseOptions();
        builder.Services.AddOptions<OutboxProcessorOptions>();
        builder.Services.AddHostedService<OutboxProcessor>();
        builder.Services.AddTransient<ISqlTransportDatabaseMigrator, PostgresDatabaseMigrator>();
        builder.Services.AddMassTransit(options =>
        {
            options.AddConsumers(assemblies);
            options.UsingPostgres((context,o) => o.ConfigureEndpoints(context));
        });
    }
    
    /// <summary>
    /// Configures and registers the necessary services for PostgreSQL database integration.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance to configure.</param>
    /// <param name="assemblies">An array of assemblies to scan for FluentMigrator migrations.</param>
    public static void AddPostgres(this WebApplicationBuilder builder, Assembly[] assemblies)
    {
        builder.AddNpgsqlDataSource("vertical-shop-db");
        builder.Services.TryAddScoped<IDatabaseContext, DatabaseContext>();
        builder.Services.TryAddTransient<DatabaseInitializer>();
        builder.Services.AddScoped<IVersionTableMetaData, CustomVersionTableMetaData>();
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
    
    /// <summary>
    /// Registers validation services by adding validators from the specified assemblies
    /// to the application service collection.
    /// </summary>
    /// <param name="builder">The instance of <see cref="WebApplicationBuilder"/> used to configure the application.</param>
    /// <param name="assemblies">An array of <see cref="System.Reflection.Assembly"/> objects from which validators will be scanned and added.</param>
    public static void AddValidation(this WebApplicationBuilder builder, Assembly[] assemblies)
    {
        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);
    }  

    private static void ConfigureMassTransitDatabaseOptions(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("vertical-shop-db");
        
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.ConnectionString = connectionString;
            options.Schema = "transport";
        });
    }
}
