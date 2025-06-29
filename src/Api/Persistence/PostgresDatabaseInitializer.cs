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

    public async Task InitializeAsync()
    {
        _migrationRunner.MigrateUp();
        await _massTransitMigrator.CreateSchemaIfNotExist(_massTransitOptions, CancellationToken.None);
        await _massTransitMigrator.CreateInfrastructure(_massTransitOptions, CancellationToken.None);
    }
}
