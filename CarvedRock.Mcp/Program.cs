using CarvedRock.Mcp;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport()    
    .WithTools<CarvedRockTools>()
    .WithTools<AdminTools>();

builder.Services.AddHttpClient("CarvedRockApi", client =>
    client.BaseAddress = new("https://api"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

app.Run();
