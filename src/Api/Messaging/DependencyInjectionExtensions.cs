using ContextDrivenDevelopment.Api.Persistence;
using MassTransit;

namespace ContextDrivenDevelopment.Api.Messaging;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, string? connectionString)
    {
        services.AddHostedService<OutboxProcessor>();
        services.AddTransient(sp => sp.GetRequiredService<IUnitOfWork>().Outbox);
        
        services.AddPostgresMigrationHostedService();
        services.ConfigureMassTransitDatabaseOptions(connectionString);
        services.AddMassTransit(options =>
        {
            options.AddConsumers(typeof(Program).Assembly);
    
            options.UsingPostgres((context,o) =>
            {
                o.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }

    private static void ConfigureMassTransitDatabaseOptions(this IServiceCollection services, string? connectionString)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        
        services.AddOptions<SqlTransportOptions>().Configure(options =>
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