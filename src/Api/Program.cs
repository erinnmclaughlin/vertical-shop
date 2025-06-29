using ContextDrivenDevelopment.Api.Domain.Inventory;
using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Persistence.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddPostgresDatabase(connectionString);
builder.Services.AddMessaging(connectionString);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

builder.Services.AddInventoryServices();
builder.Services.AddProductServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapProductApi();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetService<PostgresDatabaseInitializer>();
    if (dbInitializer is not null)
    {
        await dbInitializer.InitializeAsync(connectionString);
    }
}

app.Run();

public sealed partial class Program;