using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using CarvedRock.WebApp;
using Microsoft.AspNetCore.Identity.UI.Services;
using CarvedRock.Core;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

var authority = builder.Configuration.GetValue<string>("Auth:Authority"); 

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options => options.AccessDeniedPath = "/AccessDenied")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = authority; 
    options.ClientId = "interactive.confidential";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("api");
    options.Scope.Add("offline_access");
    options.GetClaimsFromUserInfoEndpoint = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "email"
    };
    options.SaveTokens = true;
});

builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IProductService, ProductService>();

builder.AddMailKitClient("smtp");
builder.Services.AddScoped<IEmailSender, EmailService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandler("/Error");

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserScopeMiddleware>();

app.MapRazorPages().RequireAuthorization();

app.Run();
