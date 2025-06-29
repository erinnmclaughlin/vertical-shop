using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerticalShop.Api.Persistence;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Configures and registers services for integrating with a Postgres database.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application's services and settings.</param>
    public static void AddPostgres(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Postgres");
        builder.Services.TryAddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        builder.Services.TryAddTransient(sp => sp.GetRequiredService<NpgsqlDataSource>().OpenConnection());
        builder.Services.TryAddScoped<IDatabaseContext, PostgresDatabaseContext>();
        builder.Services.TryAddTransient<PostgresDatabaseInitializer>();
        builder.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres().WithGlobalConnectionString(connectionString);
                rb.ScanIn(typeof(Program).Assembly).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}
