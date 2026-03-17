using System.Net;
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
}
