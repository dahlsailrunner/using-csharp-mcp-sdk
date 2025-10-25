var builder = DistributedApplication.CreateBuilder(args);

var carvedrockdb = builder.AddPostgres("postgres")
                          .AddDatabase("CarvedRockPostgres");

var idsrv = builder.AddProject<Projects.Duende_IdentityServer_Demo>("idsrv");

var api = builder.AddProject<Projects.CarvedRock_Api>("api")
    .WithReference(carvedrockdb)
    .WaitFor(carvedrockdb)
    .WithEnvironment("Auth__Authority", idsrv.GetEndpoint("https"))
    .WithHttpHealthCheck("/health");

var mailpit = builder.AddMailPit("smtp");

builder.AddProject<Projects.CarvedRock_WebApp>("webapp")
    .WithReference(api)
    .WithReference(mailpit)
    .WaitFor(mailpit)
    .WaitFor(api)
    .WithHttpHealthCheck("/health");

var mcp = builder.AddProject<Projects.CarvedRock_Mcp>("mcp")
    .WithReference(api)
    .WithEnvironment("AuthServer", idsrv.GetEndpoint("https"))
    .WithHttpHealthCheck("/health");

builder.AddMcpInspector("mcp-inspector")
    .WithMcpServer(mcp, path: "");

builder.Build().Run();
