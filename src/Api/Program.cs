using ContextDrivenDevelopment.Api.Inventory;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Persistence.Postgres;
using ContextDrivenDevelopment.Api.Products;
using ContextDrivenDevelopment.Api.Validation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Core infrastructure services
builder.Services.AddOpenApi();
builder.AddPostgres();
builder.AddMessaging();
builder.AddValidation();

// Application services
builder.AddInventoryServices();
builder.AddProductServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapInventoryApi();
app.MapProductApi();

// For now, just ensure the database is created during startup.
// This will change when we move to Aspire:
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetService<PostgresDatabaseInitializer>();
    if (dbInitializer is not null)
        await dbInitializer.InitializeAsync(builder.Configuration.GetConnectionString("Postgres"));
}

app.Run();

public sealed partial class Program;
