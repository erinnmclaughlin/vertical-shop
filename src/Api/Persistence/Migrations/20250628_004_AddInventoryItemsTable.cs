using FluentMigrator;

namespace VerticalShop.Api.Persistence.Migrations;

[Migration(20250628_004)]
public sealed class AddInventoryItemsTable : Migration
{
    public override void Up()
    {
        Create.Schema("inventory");
        Create.Table("items")
            .InSchema("inventory")
            .WithColumn("id").AsInt32().PrimaryKey().NotNullable().PrimaryKey().Identity()
            .WithColumn("product_slug").AsString(200).NotNullable().Unique()
            .WithColumn("quantity").AsInt32().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("items").InSchema("inventory");
        Delete.Schema("inventory");
    }
}