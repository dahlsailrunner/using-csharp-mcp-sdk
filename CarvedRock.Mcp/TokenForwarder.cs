using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http.Headers;

namespace CarvedRock.Mcp;

public class TokenForwarder(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.Authorization.FirstOrDefault() is string authHeader &&
            authHeader.StartsWith($"{JwtBearerDefaults.AuthenticationScheme} ", StringComparison.OrdinalIgnoreCase))
        {
            var tokenValue = authHeader.AsSpan(JwtBearerDefaults.AuthenticationScheme.Length + 1).Trim();
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, tokenValue.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}