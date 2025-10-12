using CarvedRock.IntegrationTests.Utils;

namespace CarvedRock.IntegrationTests;
public class HomePageTests(AppFixture fixture) : IClassFixture<AppFixture>
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    
    [Fact]
    public async Task GetCarvedRockHomeReturnsOkStatusCode()
    {
        // Arrange
        var app = fixture.App;
        //var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        //var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CarvedRock_Aspire_AppHost>(cancellationToken);

        //appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        //{
        //    clientBuilder.AddStandardResilienceHandler();
        //});

        //await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        //await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("webapp");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webapp", fixture.CancelToken).WaitAsync(_defaultTimeout, fixture.CancelToken);
        var response = await httpClient.GetAsync("/", fixture.CancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
