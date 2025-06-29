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
    /// Configures and adds messaging services to the application.
    /// </summary>
    /// <param name="builder">An instance of <see cref="WebApplicationBuilder"/> used to configure the application services.</param>
    public static void AddMessaging(this WebApplicationBuilder builder)
    {
        builder.ConfigureMassTransitDatabaseOptions();
        builder.Services.AddOptions<OutboxOptions>();
        builder.Services.AddHostedService<OutboxProcessor>();
        builder.Services.AddTransient<IOutboxPublisher, PostgresOutboxPublisher>();
        builder.Services.AddTransient<ISqlTransportDatabaseMigrator, PostgresDatabaseMigrator>();
        builder.Services.AddMassTransit(options =>
        {
            options.AddConsumers(typeof(Program).Assembly);
    
            options.UsingPostgres((context,o) =>
            {
                o.ConfigureEndpoints(context);
            });
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