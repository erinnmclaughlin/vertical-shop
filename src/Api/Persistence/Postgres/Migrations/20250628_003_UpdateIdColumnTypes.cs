using FluentMigrator;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres.Migrations;

[Migration(20250628_003)]
public class UpdateIdColumnTypes : Migration
{
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