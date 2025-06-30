using Microsoft.Extensions.Hosting;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgWeb();

if (builder.Environment.IsDevelopment())
{
    postgresServer
        .WithUserName(builder.AddParameter("pg-user", "postgres"))
        .WithPassword(builder.AddParameter("pg-password", "postgres"))
        .WithHostPort(53441);
}

var verticalShopDatabase = postgresServer
    .AddDatabase("vertical-shop-db");

var api = builder
    .AddProject<Projects.Api>("api")
    .WithReference(verticalShopDatabase)
    .WaitFor(verticalShopDatabase)
    .WithExternalHttpEndpoints();

_ = builder.AddScalarApiReference()
    .WithApiReference(api);

builder.Build().Run();
