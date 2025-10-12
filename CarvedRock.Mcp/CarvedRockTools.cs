using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CarvedRock.Mcp;

[McpServerToolType]
public class CarvedRockTools(IHttpClientFactory httpClientFactory)
{
    [McpServerTool(Name = "get_products"), Description("Get a list of all available products.")]
    public async Task<List<ProductModel>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");
        var response = await client.GetFromJsonAsync<List<ProductModel>>("/product", cancellationToken);
        return response ?? [];
    }
}
