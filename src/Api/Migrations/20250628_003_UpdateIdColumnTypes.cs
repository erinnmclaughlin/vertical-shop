using FluentMigrator;

namespace VerticalShop.Api.Migrations;

/// <inheritdoc />
[Migration(20250628_003)]
public sealed class UpdateIdColumnTypes : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Delete.ForeignKey()
            .FromTable("product_attributes").InSchema("products")
            .ForeignColumn("product_id")
            .ToTable("products").PrimaryColumn("id");
        
        Alter.Column("id")
            .OnTable("products").InSchema("products")
            .AsString(64)
            .NotNullable();
        
        Alter.Column("id")
            .OnTable("product_attributes").InSchema("products")
            .AsString(64)
            .NotNullable();
        
        Alter.Column("product_id")
            .OnTable("product_attributes").InSchema("products")
            .AsString(64)
            .NotNullable();

        Create.ForeignKey()
            .FromTable("product_attributes").InSchema("products")
            .ForeignColumn("product_id")
            .ToTable("products").InSchema("products")
            .PrimaryColumn("id");
    }

    /// <inheritdoc />
    public override void Down()
    {
        Alter.Column("id")
            .OnTable("products").InSchema("products")
            .AsGuid()
            .NotNullable();
        
        Alter.Column("id")
            .OnTable("product_attributes").InSchema("products")
            .AsGuid()
            .NotNullable();
        
        Alter.Column("product_id")
            .OnTable("product_attributes").InSchema("products")
            .AsGuid()
            .NotNullable();
    }
}