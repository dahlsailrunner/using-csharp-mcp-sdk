using CarvedRock.Mcp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;

const string McpServerUrl = "http://localhost:5241";
const string OAuthServerUrl = "https://demo.duendesoftware.com";

var backChannelHttpClient = new HttpClient
{
    BaseAddress = new Uri(OAuthServerUrl)
};

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        policy.WithExposedHeaders("mcp-session-id", "last-event-id", "mcp-protocol-version");          
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.Backchannel = backChannelHttpClient;
            options.Authority = OAuthServerUrl;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = McpServerUrl,
                ValidIssuer = OAuthServerUrl,
                NameClaimType = "name",
                RoleClaimType = "roles"
            };
        })
        .AddMcp(options =>
        {
            options.ResourceMetadata = new ProtectedResourceMetadata
            {
                Resource = new Uri(McpServerUrl),
                AuthorizationServers = { new Uri(OAuthServerUrl) },
                ScopesSupported = ["api", "openid", "profile", "email", "offline_access"]
            };
        });
builder.Services.AddAuthorization();

builder.Services.AddMcpServer()
    .WithHttpTransport()     
    .WithTools<CarvedRockTools>()
    .WithTools<AdminTools>();

builder.Services.AddHttpClient("CarvedRockApi", client =>
    client.BaseAddress = new("https://api"));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseCors("DevAll");   
}

app.MapDefaultEndpoints();


app.UseAuthentication();
app.UseAuthorization();
var mcpEndpoint = app.MapMcp();

if (app.Environment.IsDevelopment())
{
    mcpEndpoint.RequireCors("DevAll");
}
//mcpEndpoint.RequireAuthorization();

app.Run();
