using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;
using System.Reflection;
using System.Security.Claims;

namespace CarvedRock.Mcp;

public static class AuthHelper
{
    public static async Task AuthorizeToolsForUser(HttpContext ctx, McpServerOptions options, CancellationToken cancelToken)
    {
        var rolesForUser = ctx.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var allTools = options.ToolCollection;
        foreach (var toolDefinition in allTools!)
        {
            var toolMethodInfo = toolDefinition.Metadata.SingleOrDefault(m => m is MethodInfo) as MethodInfo;
            var methodClass = toolMethodInfo!.DeclaringType;

            // allow any tools with [AllowAnonymous]
            if (toolMethodInfo!.GetCustomAttribute<AllowAnonymousAttribute>() != null) continue;
            if (methodClass!.GetCustomAttribute<AllowAnonymousAttribute>() != null) continue;

            var toolAuthzAttr = toolMethodInfo!.GetCustomAttribute<AuthorizeAttribute>();

            // if there is an [Authorize] attribute of any kind and it's an anonymous user remove the tool
            if (toolAuthzAttr != null && ctx.User.Identity!.IsAuthenticated)
            {
                allTools.Remove(toolDefinition);
                continue;
            }

            if (toolAuthzAttr is null) // look for attribute on class def if it doesn't exist on method
            {
                toolAuthzAttr = methodClass!.GetCustomAttribute<AuthorizeAttribute>();
            }

            // if user has a role that the tool requires, keep it.  otherwise remove it.
            var rolesThatCanInvokeTool = toolAuthzAttr!.Roles?.Split(",");
            var userCanInvokeTool = false;
            foreach (var userRole in rolesForUser)
            {
                if (rolesThatCanInvokeTool!.Any(tr => string.Equals(tr, userRole, StringComparison.InvariantCultureIgnoreCase)))
                {
                    userCanInvokeTool = true;
                    break;
                }
            }
            if (!userCanInvokeTool)
            {
                allTools.Remove(toolDefinition);
                continue;
            }
        }

        await Task.CompletedTask;
    }
}
