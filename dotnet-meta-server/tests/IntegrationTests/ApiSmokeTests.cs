using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class ApiSmokeTests: IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _factory;

  public ApiSmokeTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task GetHealth_ReturnsOk()
  {
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/health");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}