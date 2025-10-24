using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CarvedRock.Mcp;

[Authorize(Roles = "admin")]
[McpServerToolType]
public class AdminTools(IHttpClientFactory httpClientFactory)
{
    public record OperationResult(string Status, string? Message = null);

    [McpServerTool(Name = "delete_product"), Description("Delete a single product based on its Id.")]
    public async Task<OperationResult> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");
        var response = await client.DeleteAsync($"product/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode) throw new Exception($"Error deleting product {id}; HttpResponseCode was {(int)response.StatusCode}");

        return new OperationResult("ok");
    }

    [McpServerTool(Name = "set_product_price")]
    [Description("Update the price of a single product based on its Id.")]    
    public async Task<OperationResult> UpdateProductPriceAsync(int id, double newPrice, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");

        var productToUpdate = await client.GetFromJsonAsync<FullProductModel>($"product/{id}", cancellationToken); // not found throws exception

        if (newPrice == productToUpdate!.Price)
            return new OperationResult("not changed", "new (provided) product price is same as current price");

        productToUpdate.Price = newPrice;

        var response = await client.PutAsJsonAsync($"product/{id}", productToUpdate, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode) throw new Exception($"Error updating price on product {id}; HttpResponseCode was {(int) response.StatusCode}");

        return new OperationResult("ok");
    }
}

public record FullProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
    public string ImgUrl { get; set; } = null!;
}