using CarvedRock.Core;
using CarvedRock.Mcp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var authServer = builder.Configuration.GetValue<string>("AuthServer")!;
var mcpServerUrl = builder.Configuration.GetValue<string>("McpServerUrl")!;

builder.AddServiceDefaults();

builder.Services.AddCors(options => // cors is required for mcp inspector with oauth
{
    options.AddPolicy("DevAll", policy => policy
       .AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.Authority = authServer;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidAudience = mcpServerUrl,
                ValidIssuer = authServer,
                NameClaimType = ClaimTypes.Email
            };
        })
        .AddMcp(options =>
        {            
            options.ResourceMetadata = new ProtectedResourceMetadata
            {
                Resource = new Uri(mcpServerUrl),
                AuthorizationServers = { new Uri(authServer) },
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
    .WithPromptsFromAssembly() 
    .WithToolsFromAssembly() // just get everything registered, then use authz attributes to filter
    //.WithTools<CarvedRockTools>() 
    //.WithTools<AdminTools>()
    // Authorization Filter info: https://modelcontextprotocol.github.io/csharp-sdk/concepts/filters.html#built-in-authorization-filters
    .AddAuthorizationFilters();  // Add support for [Authorize] and [AllowAnonymous]

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenForwarder>();
builder.Services.AddHttpClient("CarvedRockApi", 
        client => client.BaseAddress = new("https://api"))
    .AddHttpMessageHandler<TokenForwarder>(); ;

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
    //.RequireAuthorization();  // this would require auth for **all** connections (even "initialize")
                              // only add if you don't have any anonymous tools to support
app.Run();
