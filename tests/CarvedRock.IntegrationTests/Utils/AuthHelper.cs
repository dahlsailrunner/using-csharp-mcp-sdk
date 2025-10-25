using Duende.IdentityModel.Client;

namespace CarvedRock.IntegrationTests.Utils;
public static class AuthHelper
{
    public static async Task<string> GetUserAccessTokenAsync(AppFixture fixture,
        string username, string password,
        string scope = "openid profile email api", CancellationToken cancellationToken = default)
    {
        var idSrvRoot = fixture.App.GetEndpoint("idsrv", "https");
        var client = new HttpClient { BaseAddress = idSrvRoot };

        var response = await client.RequestPasswordTokenAsync(
            new PasswordTokenRequest
            {
                Address = "connect/token",

                ClientId = "testing.confidential",
                ClientSecret = "secret",
                Scope = scope,

                UserName = username,
                Password = password
            }, cancellationToken);

        if (response.IsError)
        {
            throw new Exception($"Error retrieving access token for user {username}: {response.Error}");
        }

        return response.AccessToken!;
    }
}
