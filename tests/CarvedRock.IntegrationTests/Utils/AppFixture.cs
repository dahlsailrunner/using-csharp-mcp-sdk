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
    }

    public async Task<McpClient> GetMcpClient(string? user = null, string? pwd = null, CancellationToken cancelToken = default)
    {
        if (user == null) return await GetAnonymousMcpClient(cancelToken);

        // must want an authenticated client
        var accessToken = await AuthHelper.GetUserAccessTokenAsync(this, user, pwd!,
            cancellationToken: cancelToken);

        var clientTransport = new HttpClientTransportOptions
        {
            Endpoint = App.GetEndpoint("mcp", "http"),
            TransportMode = HttpTransportMode.StreamableHttp,
            AdditionalHeaders = new Dictionary<string, string>()
        };
        clientTransport.AdditionalHeaders.Add("Authorization", $"Bearer {accessToken}");
        return await McpClient.CreateAsync(new HttpClientTransport(clientTransport), cancellationToken: cancelToken);
    }

    private async Task<McpClient> GetAnonymousMcpClient(CancellationToken cancelToken = default)
    {
        var clientTransport = new HttpClientTransportOptions
        {
            Endpoint = App.GetEndpoint("mcp", "http"),
            TransportMode = HttpTransportMode.StreamableHttp
        };
        return await McpClient.CreateAsync(new HttpClientTransport(clientTransport), cancellationToken: cancelToken);
    }

    public void Dispose()
    {    
        GC.SuppressFinalize(this);
    }
}
