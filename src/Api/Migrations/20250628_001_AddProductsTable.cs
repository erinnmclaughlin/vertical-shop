using FluentMigrator;

namespace VerticalShop.Api.Migrations;

/// <inheritdoc />
[Migration(20250628_001)]
public sealed class AddProductsTable : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Schema("catalog");
        
        Create.Table("products").InSchema("catalog")
            .WithColumn("id").AsString(64).PrimaryKey().NotNullable()
            .WithColumn("slug").AsString(200).NotNullable()
            .WithColumn("name").AsString(200).NotNullable();

        Create.Index("idx_products_name").OnTable("products").InSchema("catalog").OnColumn("name");
        Create.Index("idx_products_slug").OnTable("products").InSchema("catalog").OnColumn("slug").Unique();
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Index("idx_products_slug").OnTable("products").InSchema("catalog");
        Delete.Index("idx_products_name").OnTable("products").InSchema("catalog");
        Delete.Table("products").InSchema("catalog");
        Delete.Schema("catalog");
    }
}
