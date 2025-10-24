using CarvedRock.Core;
using CarvedRock.Mcp;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using System.Security.Claims;

const string McpServerUrl = "http://localhost:5241";
const string OAuthServerUrl = "https://localhost:5001";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options => // cors is required for mcp inspector with oauth
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
            options.Authority = OAuthServerUrl;
            //options.SaveToken = true;  // IMPORTANT!
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidAudience = McpServerUrl,
                ValidIssuer = OAuthServerUrl,
                NameClaimType = ClaimTypes.Email
            };
        })
        .AddMcp(options =>
        {            
            options.ResourceMetadata = new ProtectedResourceMetadata
            {
                Resource = new Uri(McpServerUrl),
                AuthorizationServers = { new Uri(OAuthServerUrl) },
                ScopesSupported = ["api", "openid", "profile", "email", "offline_access"],                
            };            
        });

builder.Services.AddAuthorization();
builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true; // important for scaling
        //options.ConfigureSessionOptions = AuthHelper.AuthorizeToolsForUser;
    })
    .WithToolsFromAssembly() // just get everything registered, then use authz attributes to filter
    //.WithTools<CarvedRockTools>() 
    //.WithTools<AdminTools>()
    .AddAuthorizationFilters();  // Add support for [Authorize] and [AllowAnonymous]

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenForwarder>();
builder.Services.AddHttpClient("CarvedRockApi", 
        client => client.BaseAddress = new("https://api"))
    .AddHttpMessageHandler<TokenForwarder>(); ;

//builder.Services.AddOpenIdConnectAccessTokenManagement();
//builder.Services.AddUserAccessTokenHttpClient("AdminApi",
//    configureClient: client => { client.BaseAddress = new Uri("https://api"); });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevAll");   
}

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserScopeMiddleware>();

var mcpEndpoint = app.MapMcp()
    .RequireAuthorization();  // this would require auth for **all** connections (even "initialize")
                                // only add if you don't have any anonymous tools to support
app.Run();
