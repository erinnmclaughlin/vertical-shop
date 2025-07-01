using FluentMigrator.Runner.VersionTableInfo;

namespace VerticalShop;

[VersionTableMetaData]
public sealed class SharedVersionTableMetaData : IVersionTableMetaData
{
    public bool OwnsSchema => true;
    public string SchemaName => "public";
    public string TableName => "migrations";
    public string ColumnName => "migration_id";
    public string DescriptionColumnName => "description";
    public string UniqueIndexName => "ux_migrations_migration_id";
    public string AppliedOnColumnName => "applied_on";
    public bool CreateWithPrimaryKey => false;
}
