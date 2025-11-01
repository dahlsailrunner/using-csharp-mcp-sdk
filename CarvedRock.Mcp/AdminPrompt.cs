using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CarvedRock.Mcp;

[McpServerPromptType]
public class CarvedRockPrompts
{
    [Authorize(Roles = "admin")]
    [McpServerPrompt(Name = "admin_prompt"), Description("A prompt without arguments")]
    public static string AdminPrompt() => 
        """
        You are an assistant that can help an administrator delete products or update the price of a product.
        Whenever you have completed an action for the administrator, confirm that the action was completed.
        """;
        
}
