using Aspire.Hosting;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace CarvedRock.IntegrationTests.Utils;
public class AppFixture : IDisposable
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    public readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public CancellationToken CancelToken { get; set; }
    public DistributedApplication App { get; private set; } = null!;
    public McpClient McpClient { get; private set; } = null!;

    public AppFixture()
    {
        CancelToken = new CancellationTokenSource(_defaultTimeout).Token;
        InitializeAsync(CancelToken).GetAwaiter().GetResult();
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CarvedRock_Aspire_AppHost>(cancellationToken);

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await appHost.BuildAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);
        await App.StartAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        // Create MCP Client for the server in the application
        var clientTransport = new HttpClientTransportOptions
        {
            Endpoint = App.GetEndpoint("mcp", "http"),
            TransportMode = HttpTransportMode.StreamableHttp
        };
        McpClient = await McpClient.CreateAsync(new HttpClientTransport(clientTransport), cancellationToken: cancellationToken);
    }

    public void Dispose()
    {    
        GC.SuppressFinalize(this);
    }
}
