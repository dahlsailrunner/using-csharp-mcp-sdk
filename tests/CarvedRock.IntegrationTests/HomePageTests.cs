using CarvedRock.IntegrationTests.Utils;

namespace CarvedRock.IntegrationTests;
public class HomePageTests(AppFixture fixture) : IClassFixture<AppFixture>
{
    [Fact]
    public async Task GetCarvedRockHomeReturnsOkStatusCode()
    {
        // Act
        var httpClient = fixture.App.CreateHttpClient("webapp");
        var response = await httpClient.GetAsync("/", fixture.CancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
