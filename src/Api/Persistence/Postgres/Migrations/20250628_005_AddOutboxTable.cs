using FluentMigrator;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres.Migrations;

[Migration(20250628_005)]
public sealed class AddOutboxTable : Migration
{
    public override void Up()
    {
        Create.Table("outbox_messages")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("type").AsString(200).NotNullable()
            .WithColumn("payload").AsCustom("jsonb").NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().WithDefault(SystemMethods.CurrentDateTimeOffset);
    }

    public override void Down()
    {
        Delete.Table("outbox_messages");
    }
}