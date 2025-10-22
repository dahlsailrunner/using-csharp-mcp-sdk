using Duende.IdentityServer.Demo;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder
    .ConfigureServices()
    .ConfigurePipeline();

app.Run();
