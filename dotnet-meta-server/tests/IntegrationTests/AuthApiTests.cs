using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Support;

namespace IntegrationTests;

public sealed class AuthApiTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public AuthApiTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetCurrentUser_WithLoginToken_ReturnsAuthenticatedUser()
    {
        var client = _fixture.Factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { account = "13800000001", password = "123456" });
        loginResponse.EnsureSuccessStatusCode();

        using var loginJson = await JsonDocument.ParseAsync(
            await loginResponse.Content.ReadAsStreamAsync());
        var accessToken = loginJson.RootElement
            .GetProperty("data")
            .GetProperty("accessToken")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var userResponse = await client.GetAsync("/api/auth/user");

        Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);

        using var userJson = await JsonDocument.ParseAsync(
            await userResponse.Content.ReadAsStreamAsync());
        var data = userJson.RootElement.GetProperty("data");

        Assert.Equal(1, data.GetProperty("id").GetInt32());
        Assert.Equal("u001", data.GetProperty("userId").GetString());
        Assert.Equal("13800000001", data.GetProperty("mobile").GetString());
    }
}
