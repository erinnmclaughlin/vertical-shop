using FluentMigrator.Runner;
using MassTransit;
using MassTransit.SqlTransport;
using Microsoft.Extensions.Options;

namespace VerticalShop.Api.Persistence;

public sealed class PostgresDatabaseInitializer(
    IMigrationRunner migrationRunner, 
    ISqlTransportDatabaseMigrator massTransitMigrator,
    IOptions<SqlTransportOptions> massTransitOptions
)
{
    private readonly IMigrationRunner _migrationRunner = migrationRunner;
    private readonly ISqlTransportDatabaseMigrator _massTransitMigrator = massTransitMigrator;
    private readonly SqlTransportOptions _massTransitOptions = massTransitOptions.Value;

    public async Task InitializeAsync(string? connectionString)
    {
        var settings = new NpgsqlConnectionStringBuilder(connectionString);

        var databaseName = settings.Database;
        
        settings.Database = "postgres"; // Use the default database to create a new one
        
        await using (var connection = new NpgsqlConnection(settings.ConnectionString))
        {
            await connection.OpenAsync();
            try { await connection.ExecuteAsync($"create database {databaseName}"); }
            catch (PostgresException) { }
        }

        _migrationRunner.MigrateUp();
        await _massTransitMigrator.CreateInfrastructure(_massTransitOptions);
    }
}
