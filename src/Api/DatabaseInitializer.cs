using Dapper;
using FluentMigrator.Runner;
using MassTransit;
using MassTransit.SqlTransport;
using Microsoft.Extensions.Options;
using Npgsql;

namespace VerticalShop.Api;

internal sealed class DatabaseInitializer(
    NpgsqlDataSource dataSource,
    IMigrationRunner migrationRunner, 
    ISqlTransportDatabaseMigrator massTransitMigrator,
    IOptions<SqlTransportOptions> massTransitOptions
)
{
    public async Task InitializeAsync()
    {
        // Create the MassTransit db schema & tables (only applicable the first time the application runs):
        await massTransitMigrator.CreateSchemaIfNotExist(massTransitOptions.Value, CancellationToken.None);
        await massTransitMigrator.CreateInfrastructure(massTransitOptions.Value, CancellationToken.None);
        
        await using (var connection = await dataSource.OpenConnectionAsync())
        {
            await connection.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
        }

        migrationRunner.MigrateUp();
    }
}
