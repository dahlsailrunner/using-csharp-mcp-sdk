using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace CarvedRock.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentController(IChatClient chatClient, IConfiguration config, IHttpContextAccessor httpCtxAccessor) : ControllerBase
{
    [HttpGet("recommendation")]
    public async IAsyncEnumerable<string> Get([EnumeratorCancellation] CancellationToken cxl)
    {
        var mcpClient = await McpClientHelper.GetMcpClient(config, httpCtxAccessor, cxl);
 
        var tools = await mcpClient.ListToolsAsync(cancellationToken: cxl);

        var agent = chatClient.CreateAIAgent(
            instructions: 
                """
                You are an assistant that can make recommendations about CarvedRock products.  
                Limit product recommendations to 3 for any request.          
                """,
            name: "CarvedRock Assistant",
            tools: [.. tools]);

        await foreach (var update in agent.RunStreamingAsync(
            "I've got a hike coming up on a mostly-paved path.  Can you give me some product recommendations?", 
            cancellationToken: cxl))
        {
            yield return update.ToString();
        }
    }
}
