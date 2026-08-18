using System.Net;
using IntegrationTests.Support;

namespace IntegrationTests;

public class ApiSmokeTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public ApiSmokeTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}