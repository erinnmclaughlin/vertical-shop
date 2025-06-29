using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds Postgres-related services to the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// An instance of <see cref="IServiceCollection"/> to register services with.
    /// </param>
    /// <param name="connectionString">
    /// The connection string required to connect to the Postgres database.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> instance with the Postgres database services registered.
    /// </returns>
    public static IServiceCollection AddPostgresDatabase(this IServiceCollection services, string? connectionString)
    {
        services.TryAddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.TryAddTransient(sp => sp.GetRequiredService<NpgsqlDataSource>().OpenConnection());
        services.TryAddScoped<IUnitOfWork, PostgresUnitOfWork>();
        services.TryAddTransient<PostgresDatabaseInitializer>();
        
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres().WithGlobalConnectionString(connectionString);
                rb.ScanIn(typeof(Program).Assembly).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());
        
        return services;
    }
}
