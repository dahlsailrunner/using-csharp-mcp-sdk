using CarvedRock.IntegrationTests.Utils;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace CarvedRock.IntegrationTests;
public class McpServerTests(AppFixture fixture) : IClassFixture<AppFixture>
{
    [Fact]    
    public async Task AnonymousConnectionThrowsException()
    {
        try
        {
            var mcpClient = await fixture.GetMcpClient(cancelToken: fixture.CancelToken);
        }
        catch (Exception ex)
        {
            Assert.Contains("401 (Unauthorized)", ex.Message);
            return;
        }
        Assert.Fail("Expected an McpException to be thrown.");
    }

    [Theory]
    [InlineData("alice", "alice")]
    [InlineData("bob", "bob")]
    public async Task GetToolsIncludesGetProducts(string user, string pwd)
    {
        var mcpClient = await fixture.GetMcpClient(user, pwd, fixture.CancelToken);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: fixture.CancelToken);

        // Assert
        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        Assert.NotNull(getProductsTool);
    }

    [Fact]
    public async Task CallGetProductsToolReturnsProducts()
    {
        var mcpClient = await fixture.GetMcpClient("alice", "alice", fixture.CancelToken);

        //Act
        var getProductsResponse = await mcpClient.CallToolAsync(
            "get_products", cancellationToken: fixture.CancelToken);

        //Assert
        Assert.NotNull(getProductsResponse);
        Assert.NotEqual(true, getProductsResponse.IsError);

        var productJson = getProductsResponse.Content.First(c => c.Type == "text") as TextContentBlock;
        var products = JsonSerializer.Deserialize<List<ProductModel>>(
            productJson?.Text ?? "[]",
            fixture.JsonSerializerOptions);

        Assert.NotNull(products);
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

    [Fact]
    public async Task DeleteProductWorksForBob()
    {
        var mcpClient = await fixture.GetMcpClient("bob", "bob", fixture.CancelToken);

        var response = await mcpClient.CallToolAsync("delete_product", 
            new Dictionary<string, object?> 
            { 
                {"id", 22} 
            },
            cancellationToken: fixture.CancelToken);

        var responseJson = response.Content.First(c => c.Type == "text") as TextContentBlock;
        var opResult = JsonSerializer.Deserialize<OperationResult>(responseJson?.Text ?? "{}",
            fixture.JsonSerializerOptions);

        Assert.NotNull(response);
        Assert.Equal("ok", opResult?.Status);
    }

    [Fact]
    public async Task DeleteProductDoesNotWorkForAlice()
    {
        var mcpClient = await fixture.GetMcpClient("alice", "alice", fixture.CancelToken);

        try 
        {
            var response = await mcpClient.CallToolAsync("delete_product",
            new Dictionary<string, object?>
            {
                {"id", 1}
            },
            cancellationToken: fixture.CancelToken);
        }
        catch (McpException ex)
        {
            Assert.Equal(McpErrorCode.InvalidRequest, ex.ErrorCode);
            Assert.Contains("requires authorization", ex.Message);
            return;
        }
        Assert.Fail("Expected an McpException to be thrown.");
    }
}

public record OperationResult(string Status, string? Message = null);

public record ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
}