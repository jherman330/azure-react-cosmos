using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Todo.Api.Tests.Integration;

/// <summary>
/// REQ-FOUNDATION-005: distributed write-tier limit, 429 + Retry-After + rate limit headers.
/// </summary>
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class RateLimitingTests
{
    [Fact]
    public async Task WriteTier_ThirdRequestWithinWindow_Returns429_WithRetryAfterAndRateLimitHeaders()
    {
        using var factory = new WebApplicationFactory<global::Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:Write:PermitLimit"] = "2",
                });
            });
        });

        var client = factory.CreateClient();
        const string url = "/api/v1/sandbox/validate";
        var body = new { message = "rate-limit-test" };

        using var first = await client.PostAsJsonAsync(url, body);
        using var second = await client.PostAsJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal("2", Assert.Single(first.Headers.GetValues("X-RateLimit-Limit")));
        Assert.Equal("1", Assert.Single(first.Headers.GetValues("X-RateLimit-Remaining")));
        Assert.Equal("2", Assert.Single(second.Headers.GetValues("X-RateLimit-Limit")));
        Assert.Equal("0", Assert.Single(second.Headers.GetValues("X-RateLimit-Remaining")));

        using var third = await client.PostAsJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.True(third.Headers.Contains("Retry-After"));
        Assert.Equal("2", Assert.Single(third.Headers.GetValues("X-RateLimit-Limit")));
        Assert.Equal("0", Assert.Single(third.Headers.GetValues("X-RateLimit-Remaining")));
    }
}
