using System.Reflection;
using VerticalShop.Api.Messaging;
using VerticalShop.Api.Persistence;
using VerticalShop.Api.Products;
using VerticalShop.Api.Validation;
using VerticalShop.Inventory;

var builder = WebApplication.CreateBuilder(args);

Assembly[] moduleAssemblies = 
[
    builder.AddProductModule(),
    builder.AddInventoryModule()
];

// Core infrastructure services
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddPostgres(moduleAssemblies);
builder.AddMessaging(moduleAssemblies);
builder.AddValidation(moduleAssemblies);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapInventoryApi();
app.MapProductApi();

// For now, just ensure the database is created during startup.
// This will change when we move to Aspire:
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetService<PostgresDatabaseInitializer>();
    if (dbInitializer is not null)
        await dbInitializer.InitializeAsync();
}

app.Run();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed partial class Program;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
