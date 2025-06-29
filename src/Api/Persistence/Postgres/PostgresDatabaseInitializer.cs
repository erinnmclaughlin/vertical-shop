using FluentMigrator.Runner;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

public sealed class PostgresDatabaseInitializer
{
    private readonly IMigrationRunner _migrationRunner;
    
    public PostgresDatabaseInitializer(IMigrationRunner migrationRunner)
    {
        _migrationRunner = migrationRunner;
    }

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
    }
}
