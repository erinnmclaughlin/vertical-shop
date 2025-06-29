using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder
    .AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

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
