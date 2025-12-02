var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Organization_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Organization_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.Organization_Blazor>("blazorfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
