using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Scalar.AspNetCore;
using VerticalShop.Api;
using VerticalShop.Catalog;
using VerticalShop.Inventory;

var builder = WebApplication.CreateBuilder(args);

Assembly[] moduleAssemblies = 
[
    builder.AddCatalogModule(),
    builder.AddInventoryModule()
];

builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddPostgres(moduleAssemblies);
builder.AddMessaging(moduleAssemblies);
builder.AddValidation(moduleAssemblies);

builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblies(moduleAssemblies);
});

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

// For now, just ensure the database is created during startup.
// This will change when we move to Aspire:
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
