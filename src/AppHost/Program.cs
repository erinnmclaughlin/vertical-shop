using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder
    .AddProject<Projects.Api>("api")
    .WithExternalHttpEndpoints();

var scalar = builder.AddScalarApiReference()
    .WithApiReference(api);

builder.Build().Run();
