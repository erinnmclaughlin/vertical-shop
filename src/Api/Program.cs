using ContextDrivenDevelopment.Api.Domain.Inventory;
using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Persistence;
using ContextDrivenDevelopment.Api.Persistence.Postgres;
using Dapper;
using FluentMigrator.Runner;
using FluentValidation;
using MassTransit;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
{
    var connectionString = new NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("Postgres"));

    options.Host = connectionString.Host;
    options.Port = connectionString.Port;
    options.Database = connectionString.Database;
    options.Schema = "transport";
    options.Role = "transport";
    options.Username = "masstransit";
    options.Password = "H4rd2Gu3ss!";

    // credentials to run migrations
    options.AdminUsername = connectionString.Username;
    options.AdminPassword = connectionString.Password;
});
// MassTransit will run the migrations on start up
builder.Services.AddPostgresMigrationHostedService();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    
    x.UsingPostgres((context,cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddPostgresDatabase(connectionString);
builder.Services.AddTransient(sp => sp.GetRequiredService<IUnitOfWork>().Outbox);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddInventoryServices();
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