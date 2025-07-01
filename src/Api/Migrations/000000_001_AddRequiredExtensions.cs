using FluentMigrator;

namespace VerticalShop.OutboxProcessor.Migrations;

/// <inheritdoc />
[Migration(000000_001)]
public class AddRequiredExtensions : Migration 
{
    /// <inheritdoc />
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";\n");
    }

    /// <inheritdoc />
    public override void Down()
    {
        return;
    }
}
