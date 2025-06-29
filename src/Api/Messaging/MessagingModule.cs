using System.Reflection;
using MassTransit;
using MassTransit.SqlTransport;
using MassTransit.SqlTransport.PostgreSql;

namespace VerticalShop.Api.Messaging;

/// <summary>
/// Provides methods to configure messaging services in the application.
/// </summary>
public static class MessagingModule
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