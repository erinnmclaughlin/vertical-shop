using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator(azurite => azurite.WithDataVolume());
var blobs = storage.AddBlobs("blobs");

var postgresServer = builder
    .AddPostgres("postgres")
    .WithPgWeb();

if (builder.Environment.IsDevelopment() && builder.Configuration["KeepPostgresContainerAlive"] == "true")
{
    postgresServer
        .WithDataVolume()
        .WithUserName(builder.AddParameter("pg-user", "postgres"))
        .WithPassword(builder.AddParameter("pg-password", "postgres"))
        .WithHostPort(5432)
        .WithLifetime(ContainerLifetime.Persistent);
}

var verticalShopDatabase = postgresServer
    .AddDatabase("vertical-shop-db");

var api = builder
    .AddProject<Projects.Api>("api")
    .WithReference(verticalShopDatabase)
    //.WithReference(blobs)
    //.WaitFor(blobs)
    .WaitFor(verticalShopDatabase)
    .WithExternalHttpEndpoints();

var outboxProcessor = builder
    .AddProject<Projects.OutboxProcessor>("outbox-processor")
    .WithReference(verticalShopDatabase)
    .WaitFor(api);

// The Scalar container for Aspire doesn't work on Mac :(
if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    _ = builder.AddScalarApiReference().WithApiReference(api);
}

builder.Build().Run();
