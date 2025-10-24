using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CarvedRock.Mcp;

[AllowAnonymous]
[McpServerToolType]
public class CarvedRockTools(IHttpClientFactory httpClientFactory)
{
    [McpServerTool(Name = "get_products"), Description("Get a list of all available products.")]
    public async Task<List<ProductModel>> GetAllProductsAsync(
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");
        var response = await client.GetFromJsonAsync<List<ProductModel>>("/product", 
            cancellationToken);
        return response ?? [];
    }

    // might not want this one.  depends on your use case(s).  definitely don't just add all methods of an API --
    // only add what you intend your AI service to use.
    [McpServerTool(Name = "get_single_product"), Description("Get a single product based on an Id.")]
    public async Task<ProductModel?> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");
        return await client.GetFromJsonAsync<ProductModel>($"/product/{id}", cancellationToken);
    }
}
