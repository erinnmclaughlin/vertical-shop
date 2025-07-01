using FluentMigrator.Runner.VersionTableInfo;

namespace VerticalShop;

public abstract class BaseVersionTableMetaData: IVersionTableMetaData
{
    public abstract string SchemaName { get; }
    
    public bool OwnsSchema => true;
    public string TableName => "migration_history";
    public string ColumnName => "migration_id";
    public string DescriptionColumnName => "migration_name";
    public string UniqueIndexName => "ux_migration_history_migration_id";
    public string AppliedOnColumnName => "applied_on";
    public bool CreateWithPrimaryKey => true;
}
