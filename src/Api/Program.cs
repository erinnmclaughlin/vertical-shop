using System.Reflection;
using FluentValidation;
using MassTransit;
using Scalar.AspNetCore;
using VerticalShop;
using VerticalShop.Api;
using VerticalShop.Catalog;
using VerticalShop.Inventory;

var builder = WebApplication.CreateBuilder(args);

Assembly[] moduleAssemblies = 
[
    builder.AddCatalogApi(),
    typeof(InventoryApi).Assembly
];

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("vertical-shop-db");
builder.AddDatabaseInitialization(moduleAssemblies);
builder.AddMassTransit(x => x.AddConsumers(moduleAssemblies));

builder.Services.AddSharedServices();
builder.Services.AddOpenApi();
builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblies(moduleAssemblies);
    x.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        // https://github.com/scalar/scalar/discussions/4025
        o.Servers = [];
    });
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapInventoryApi();
app.MapCatalogApi();

// For now, just ensure the database is created during startup. Eventually we can move this to a worker service or similar.
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetService<DatabaseInitializer>();
    if (dbInitializer is not null)
        await dbInitializer.InitializeAsync();
}
app.Run();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed partial class Program;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
