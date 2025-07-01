using FluentMigrator;

// TODO: Ideally this would live in the "OutboxProcessor" assembly, but waiting until
// there's a dedicated worker service for initializing / migrating the database

namespace VerticalShop.OutboxProcessor.Migrations;

/// <inheritdoc />
[Migration(20250628_005)]
public sealed class AddOutboxTable : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        Create.Table("outbox_messages")
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewSequentialId).PrimaryKey()
            .WithColumn("type").AsString(200).NotNullable()
            .WithColumn("payload").AsCustom("jsonb").NotNullable()
            .WithColumn("created_on_utc").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("processed_on_utc").AsDateTimeOffset().Nullable()
            .WithColumn("error_message").AsString().Nullable();
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Table("outbox_messages");
    }
}
