using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class DiagnosticsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DiagnosticsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Success_ReturnsUnifiedResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetFromJsonAsync<ApiEnvelope<object>>("/api/diagnostics/success");

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("OK", response.Code);
        Assert.Equal("success", response.Message);
        Assert.False(string.IsNullOrWhiteSpace(response.RequestId));
    }

    [Fact]
    public async Task BusinessError_ReturnsStableError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/business-error");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("DEMO_BUSINESS_ERROR", body.Code);
        Assert.Equal("这是一个业务异常示例", body.Message);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task ServerError_ReturnsUnifiedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/server-error");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("INTERNAL_SERVER_ERROR", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task SecureEndpoint_ReturnsUnifiedUnauthorizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/secure");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("UNAUTHORIZED", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task ValidationError_ReturnsUnifiedBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/diagnostics/validation", new { });
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("VALIDATION_ERROR", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task RequestId_CanBeProvidedByHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/diagnostics/success");
        request.Headers.Add("X-Request-Id", "test-request-001");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal("test-request-001", response.Headers.GetValues("X-Request-Id").Single());
        Assert.NotNull(body);
        Assert.Equal("test-request-001", body.RequestId);
    }

    private sealed record ApiEnvelope<T>(
        bool Success,
        string Code,
        string Message,
        T? Data,
        string RequestId);
}