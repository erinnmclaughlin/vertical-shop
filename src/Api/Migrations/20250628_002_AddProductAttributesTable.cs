using FluentMigrator;

namespace VerticalShop.Api.Migrations;

/// <inheritdoc />
[Migration(20250628_002)]
public sealed class AddProductAttributesTable : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Table("product_attributes")
            .InSchema("catalog")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("product_id").AsString(64).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("value").AsString(200).NotNullable();
        
        Create.Index("fk_product_attributes_product_id")
            .OnTable("product_attributes").InSchema("catalog")
            .OnColumn("product_id");
        
        Create.ForeignKey()
            .FromTable("product_attributes").InSchema("catalog")
            .ForeignColumn("product_id")
            .ToTable("products").InSchema("catalog")
            .PrimaryColumn("id");
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.ForeignKey().FromTable("product_attributes").InSchema("catalog").ForeignColumn("product_id");
        Delete.Index("fk_product_attributes_product_id").OnTable("product_attributes").InSchema("catalog");
        Delete.Table("product_attributes").InSchema("catalog");
    }
}
