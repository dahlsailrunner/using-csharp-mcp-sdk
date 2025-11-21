using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarvedRock.Api;

public class SwaggerOptions(IConfiguration config) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var authority = config.GetValue<string>("Auth:Authority");
        var oauthScopes = new Dictionary<string, string>
            {
                { "api", "Resource access: Carved Rock API" },
                { "openid", "OpenID information"},
                { "profile", "User profile information" },
                { "email", "User email address" }
            };
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/connect/authorize"),
                    TokenUrl = new Uri($"{authority}/connect/token"),
                    Scopes = oauthScopes
                }
            }
        });

        options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
        {
            [new OpenApiSecuritySchemeReference("oauth2", document)] = [.. oauthScopes.Keys]
        });
    }
}