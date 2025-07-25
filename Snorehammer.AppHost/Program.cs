var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Snorehammer_ApiService>("apiservice");

builder.AddProject<Projects.Snorehammer_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
