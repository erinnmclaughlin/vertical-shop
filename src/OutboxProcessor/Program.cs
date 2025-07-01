using VerticalShop;
using VerticalShop.OutboxProcessor;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.AddNpgsqlDataSource("vertical-shop-db");
builder.AddMassTransit();
builder.AddServiceDefaults();
builder.Build().Run();
