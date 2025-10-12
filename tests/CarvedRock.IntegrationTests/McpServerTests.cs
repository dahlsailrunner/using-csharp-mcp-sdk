using CarvedRock.IntegrationTests.Utils;
using ModelContextProtocol.Protocol;

namespace CarvedRock.IntegrationTests;
public class McpServerTests(AppFixture fixture) : IClassFixture<AppFixture>
{
    //private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task ListToolsHasGetProducts()
    {
        //// Arrange
        //var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        //var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CarvedRock_Aspire_AppHost>(cancellationToken);

        //appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        //{
        //    clientBuilder.AddStandardResilienceHandler();
        //});

        //await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        //await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        //var app = fixture.App;
        //var endpoint = app.GetEndpoint("mcp", "http");

        //// Act
        //var clientTransport = new HttpClientTransportOptions
        //{
        //    Endpoint = endpoint,
        //    TransportMode = HttpTransportMode.StreamableHttp
        //};

        //var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(clientTransport));

        var tools = await fixture.McpClient.ListToolsAsync(cancellationToken: fixture.CancelToken);
        Assert.NotNull(tools);

        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        Assert.NotNull(getProductsTool);        
    }

    [Fact]
    public async Task CallGetProductsToolReturnsProducts()
    {
        //Arrange
        var mcpClient = fixture.McpClient;        

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
}

public record ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
}