using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using VerticalShop;
using VerticalShop.OutboxProcessor;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.AddNpgsqlDataSource("vertical-shop-db");
builder.AddMassTransit();
builder.AddServiceDefaults();

// this sets up the FluentMigrator Postgres runner to apply migrations (used by the DatabaseInitializer):
builder.Services
    .AddFluentMigratorCore()
    .AddScoped<IVersionTableMetaData, SharedVersionTableMetaData>()
    .ConfigureRunner(rb =>
    {
        rb.AddPostgres();
        rb.WithGlobalConnectionString(builder.Configuration.GetConnectionString("vertical-shop-db"));
        rb.ScanIn(typeof(Program).Assembly).For.All();
    })
    .AddLogging(lb => lb.AddFluentMigratorConsole());

builder.Build().Run();
