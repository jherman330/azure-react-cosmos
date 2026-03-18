using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Todo.Api.Tests.Integration;

/// <summary>
/// Example integration test using WebApplicationFactory for full HTTP pipeline (AC-FOUNDATION-012.3, 012.7).
/// </summary>
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<global::Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            })
            .CreateClient();
    }

    [Fact]
    public async Task Health_Returns_200()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Root_Returns_Success()
    {
        // In this app, "/" may serve Swagger UI (RoutePrefix "") or the minimal API; either way we expect success.
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Live_Returns_200_With_Status_And_Duration_Without_Dependency_Checks()
    {
        var response = await _client.GetAsync("/health/live");
        response.EnsureSuccessStatusCode();
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("duration", out _));
        Assert.False(root.TryGetProperty("checks", out _));
    }

    [Fact]
    public async Task Ready_Returns_200_In_Testing_With_Checks_Object()
    {
        var response = await _client.GetAsync("/health/ready");
        response.EnsureSuccessStatusCode();
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("duration", out _));
        var checks = root.GetProperty("checks");
        Assert.True(checks.TryGetProperty("dependencies", out _) || checks.TryGetProperty("cosmos", out _) || checks.TryGetProperty("redis", out _));
    }
}
