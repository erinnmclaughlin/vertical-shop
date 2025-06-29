using FluentMigrator;

namespace VerticalShop.Api.Migrations;

/// <inheritdoc />
[Migration(20250628_001)]
public sealed class AddProductsTable : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Schema("products");
        
        Create.Table("products")
            .InSchema("products")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("slug").AsString(200).NotNullable()
            .WithColumn("name").AsString(200).NotNullable();

        Create.Index("idx_products_name").OnTable("products").InSchema("products").OnColumn("name");
        Create.Index("idx_products_slug").OnTable("products").InSchema("products").OnColumn("slug").Unique();
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Index("idx_products_slug").OnTable("products").InSchema("products");
        Delete.Index("idx_products_name").OnTable("products").InSchema("products");
        Delete.Table("products").InSchema("products");
        Delete.Schema("products");
    }
}
