using FluentMigrator;

namespace VerticalShop.Api.Migrations;

/// <inheritdoc />
[Migration(20250630_001)]
public class AddProducePricesTable : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Table("product_prices").InSchema("catalog")
            .WithColumn("id").AsString(64).PrimaryKey().NotNullable()
            .WithColumn("product_id").AsString(64).NotNullable()
            .WithColumn("price").AsDecimal(10, 2).NotNullable()
            .WithColumn("valid_from").AsDateTimeOffset().NotNullable()
            .WithColumn("valid_to").AsDateTimeOffset().Nullable();
        
        Create.ForeignKey()
            .FromTable("product_prices").InSchema("catalog")
            .ForeignColumn("product_id")
            .ToTable("products").InSchema("catalog")
            .PrimaryColumn("id");
        
        Create.Index("idx_product_prices_product_id").OnTable("product_prices").InSchema("catalog").OnColumn("product_id");
        Create.Index("idx_product_prices_valid_from").OnTable("product_prices").InSchema("catalog").OnColumn("valid_from");
        Create.Index("idx_product_prices_valid_to").OnTable("product_prices").InSchema("catalog").OnColumn("valid_to");

        // ensure only one price is active for a given product at a time
        Execute.Sql(
            """
            CREATE EXTENSION IF NOT EXISTS btree_gist;
            ALTER TABLE catalog.product_prices ADD CONSTRAINT no_overlapping_prices
            EXCLUDE USING gist (
                product_id WITH =,
                tstzrange(valid_from, valid_to) WITH &&
            );
            """);

    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Table("product_prices").InSchema("catalog");
    }
}
