using FluentMigrator;

namespace VerticalShop.Catalog.Migrations;

/// <inheritdoc />
[Migration(20250630_001)]
public sealed class InitialCatalogSchema : Migration 
{
    /// <inheritdoc />
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";\n");
        Create.Schema("catalog");
        CreateProductsTable();
        CreateProductVariantsTable();
        CreateCatalogsTable();
        CreateCatalogItemsTable();
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Table("catalog_items").InSchema("catalog");
        Delete.Table("catalogs").InSchema("catalog");
        Delete.Table("product_variants").InSchema("catalog");
        Delete.Table("products").InSchema("catalog");
        
        Delete.Schema("catalog");
    }

    private void CreateProductsTable()
    {
        Create.Table("products").InSchema("catalog")
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewSequentialId).PrimaryKey()
            .WithColumn("slug").AsString(200).NotNullable().Unique()
            .WithColumn("name").AsString(200).NotNullable().Indexed();
    }
    
    private void CreateProductVariantsTable()
    {
        Create.Table("product_variants").InSchema("catalog")
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewSequentialId).PrimaryKey()
            .WithColumn("product_id").AsGuid().Indexed()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("attributes").AsCustom("jsonb");
        
        Create.ForeignKey()
            .FromTable("product_variants").InSchema("catalog").ForeignColumn("product_id")
            .ToTable("products").InSchema("catalog").PrimaryColumn("id");
    }
    
    private void CreateCatalogsTable()
    {
        Create.Table("catalogs").InSchema("catalog")
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewSequentialId).PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("start_date").AsDateTime().NotNullable()
            .WithColumn("end_date").AsDateTime().Nullable()
            .WithColumn("is_default").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Index()
            .OnTable("catalogs").InSchema("catalog")
            .OnColumn("start_date").Ascending()
            .OnColumn("end_date").Ascending();
        
        // ensure only one catalog is marked as default at one time
        Execute.Sql(
            """
            CREATE UNIQUE INDEX ux_catalog_default
            ON catalog.catalogs (is_default)
            WHERE is_default = true;
            """
        );
        
        // ensure there is always a default catalog
        Insert.IntoTable("catalogs").InSchema("catalog")
            .Row(new { id = Guid.Empty, name = "Default", start_date = DateTimeOffset.UtcNow, is_default = true });
    }

    private void CreateCatalogItemsTable()
    {
        Create.Table("catalog_items").InSchema("catalog")
            .WithColumn("catalog_id").AsGuid().Indexed()
            .WithColumn("variant_id").AsGuid().Indexed()
            .WithColumn("currency").AsString(3).NotNullable().WithDefaultValue("USD")
            .WithColumn("price").AsDecimal(10, 2).NotNullable();

        Create.PrimaryKey()
            .OnTable("catalog_items").WithSchema("catalog")
            .Columns("catalog_id", "variant_id", "currency");

        Create.ForeignKey()
            .FromTable("catalog_items").InSchema("catalog").ForeignColumn("catalog_id")
            .ToTable("catalogs").InSchema("catalog").PrimaryColumn("id");

        Create.ForeignKey()
            .FromTable("catalog_items").InSchema("catalog").ForeignColumn("variant_id")
            .ToTable("product_variants").InSchema("catalog").PrimaryColumn("id");
    }
}
