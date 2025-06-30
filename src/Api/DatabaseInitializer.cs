using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MassTransit;
using MassTransit.SqlTransport;
using Microsoft.Extensions.Options;

namespace VerticalShop.Api;

internal sealed class DatabaseInitializer(
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

[VersionTableMetaData]
internal sealed class CustomVersionTableMetaData : IVersionTableMetaData
{
    public bool OwnsSchema => false;
    public string SchemaName => "public";
    public string TableName => "migrations";
    public string ColumnName => "migration_id";
    public string DescriptionColumnName => "description";
    public string UniqueIndexName => "ux_migrations_migration_id";
    public string AppliedOnColumnName => "applied_on";
    public bool CreateWithPrimaryKey => false;
}
