using CarvedRock.IntegrationTests.Utils;
using ModelContextProtocol.Protocol;

namespace CarvedRock.IntegrationTests;
public class McpServerTests(AppFixture fixture) : IClassFixture<AppFixture>
{
    [Fact]
    public async Task ListToolsHasGetProducts()
    {
        // get an anonymous client
        var mcpClient = await fixture.GetMcpClient(cancelToken: fixture.CancelToken);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: fixture.CancelToken);
        Assert.NotNull(tools);

        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        Assert.NotNull(getProductsTool);        
    }

    [Fact]
    public async Task CallGetProductsToolReturnsProducts()
    {
        // get an anonymous client
        var mcpClient = await fixture.GetMcpClient(cancelToken: fixture.CancelToken);

        //Act
        var getProductsResponse = await mcpClient.CallToolAsync("get_products", cancellationToken: fixture.CancelToken);

        //Assert
        Assert.NotNull(getProductsResponse);        
        Assert.NotEqual(true, getProductsResponse.IsError); // iserror is nullable bool

        var productJson = getProductsResponse.Content.First(c => c.Type == "text") as TextContentBlock;
        var products = System.Text.Json.JsonSerializer.Deserialize<List<ProductModel>>(productJson?.Text ?? "[]",
            fixture.JsonSerializerOptions);

        Assert.NotNull(products);
        Assert.Equal(50, products?.Count);
        Assert.Contains(products!, p => p.Name == "Alpine Trekker");
    }

    [Fact]
    public async Task ListToolsDoesNotHaveAdminToolsForAlice()
    {
        var mcpClient = await fixture.GetMcpClient("alice", "alice", fixture.CancelToken);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: fixture.CancelToken);
        Assert.NotNull(tools);
        Assert.Equal(2, tools.Count);

        var adminTool = tools.FirstOrDefault(t => t.Name == "delete_product");
        Assert.Null(adminTool);

        adminTool = tools.FirstOrDefault(t => t.Name == "set_product_price");
        Assert.Null(adminTool);
    }

    [Fact]
    public async Task ListToolsHasAdminToolsForBob()
    {
        var mcpClient = await fixture.GetMcpClient("bob", "bob", fixture.CancelToken);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: fixture.CancelToken);
        Assert.NotNull(tools);
        Assert.Equal(4, tools.Count);

        var adminTool = tools.FirstOrDefault(t => t.Name == "delete_product");
        Assert.NotNull(adminTool);

        adminTool = tools.FirstOrDefault(t => t.Name == "set_product_price");
        Assert.NotNull(adminTool);
    }
}

public record ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
}