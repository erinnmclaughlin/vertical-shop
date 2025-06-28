using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Persistence.Postgres;
using Dapper;
using FluentMigrator.Runner;
using FluentValidation;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddPostgresDatabase(connectionString);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddProductServices();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapProductApi();

// Ensure database exists
var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres" };
using (var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString))
{
    connection.Open();
    try { connection.Execute("create database context_driven_development"); }
    catch (PostgresException) { }
}

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
}

app.Run();

public sealed partial class Program;