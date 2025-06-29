using FluentMigrator;

namespace VerticalShop.Api.Persistence.Migrations;

/// <inheritdoc />
[Migration(20250628_002)]
public sealed class AddProductAttributesTable : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Table("product_attributes")
            .InSchema("products")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("product_id").AsGuid().NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("value").AsString(200).NotNullable();
        
        Create.Index("fk_product_attributes_product_id")
            .OnTable("product_attributes").InSchema("products")
            .OnColumn("product_id");
        
        Create.ForeignKey()
            .FromTable("product_attributes").InSchema("products")
            .ForeignColumn("product_id")
            .ToTable("products").InSchema("products")
            .PrimaryColumn("id");
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.ForeignKey().FromTable("product_attributes").InSchema("products").ForeignColumn("product_id");
        Delete.Index("fk_product_attributes_product_id").OnTable("product_attributes").InSchema("products");
        Delete.Table("product_attributes").InSchema("products");
    }
}
