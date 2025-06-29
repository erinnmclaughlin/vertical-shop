using MassTransit;
using MassTransit.SqlTransport;
using MassTransit.SqlTransport.PostgreSql;

namespace VerticalShop.Api.Messaging;

public static class DependencyInjectionExtensions
{
    public static void AddMessaging(this WebApplicationBuilder builder)
    {
        builder.ConfigureMassTransitDatabaseOptions();
        builder.Services.AddOptions<OutboxOptions>();
        builder.Services.AddHostedService<OutboxProcessor>();
        builder.Services.AddTransient<IOutbox, PostgresOutbox>();
        builder.Services.AddTransient<ISqlTransportDatabaseMigrator, PostgresDatabaseMigrator>();
        builder.Services.AddOptions<SqlTransportOptions>();
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
        var connectionString = builder.Configuration.GetConnectionString("Postgres");
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.Host = connectionStringBuilder.Host;
            options.Port = connectionStringBuilder.Port;
            options.Database = connectionStringBuilder.Database;
            options.Schema = "transport";
            options.Role = "transport";
            options.Username = "masstransit";
            options.Password = "H4rd2Gu3ss!";

            // credentials to run migrations
            options.AdminUsername = connectionStringBuilder.Username;
            options.AdminPassword = connectionStringBuilder.Password;
        });
    }
}