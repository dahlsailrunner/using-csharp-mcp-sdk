using CarvedRock.Core;
using CarvedRock.Mcp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;

const string McpServerUrl = "http://localhost:5241";
const string OAuthServerUrl = "https://localhost:5001";

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
                ValidateIssuerSigningKey = true,
                ValidAudience = McpServerUrl,
                ValidIssuer = OAuthServerUrl
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
builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
        //options.ConfigureSessionOptions = AuthHelper.AuthorizeToolsForUser;
    })
    .WithToolsFromAssembly()
    //.WithTools<CarvedRockTools>() 
    //.WithTools<AdminTools>()
    .AddAuthorizationFilters();  // Add support for [Authorize] and [AllowAnonymous]

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

app.UseMiddleware<UserScopeMiddleware>();

var mcpEndpoint = app.MapMcp();
    //.RequireAuthorization();  // this would require auth for **all** connections

app.Run();
