using FluentMigrator.Runner.VersionTableInfo;

namespace VerticalShop.Api.Persistence;

[VersionTableMetaData]
internal sealed class CustomVersionTableMetaData : IVersionTableMetaData
{
    public bool OwnsSchema => true;
    public string SchemaName => "public";
    public string TableName => "version_info";
    public string ColumnName => "version";
    public string DescriptionColumnName => "description";
    public string UniqueIndexName => "ux_version_info_version";
    public string AppliedOnColumnName => "applied_on";
    public bool CreateWithPrimaryKey => true;
}