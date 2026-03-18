using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Todo.Api.Tests.Integration;

/// <summary>
/// AC-FOUNDATION-006: CORS default policy by environment and configuration.
/// </summary>
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class CorsConfigurationTests
{
    private const string AllowedTestOrigin = "https://cors-allowed.test";
    private const string OtherOrigin = "https://other.test";

    private static WebApplicationFactory<global::Program> FactoryDevelopment() =>
        new WebApplicationFactory<global::Program>().WithWebHostBuilder(b =>
            b.UseEnvironment("Development"));

    private static WebApplicationFactory<global::Program> FactoryRestricted(string allowedOrigins)
    {
        var json = JsonSerializer.Serialize(new { Cors = new { AllowedOrigins = allowedOrigins } });
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return new WebApplicationFactory<global::Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Production");
            b.ConfigureAppConfiguration((_, cfg) => cfg.AddJsonStream(stream));
        });
    }

    [Fact]
    public async Task Development_WithOrigin_AllowsAnyOrigin()
    {
        await using var factory = FactoryDevelopment();
        using var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation("Origin", OtherOrigin);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains("*", values);
    }

    [Fact]
    public async Task NonDevelopment_AllowsOnlyConfiguredOrigins()
    {
        await using var factory = FactoryRestricted(AllowedTestOrigin);
        using var client = factory.CreateClient();

        var allowed = new HttpRequestMessage(HttpMethod.Get, "/health");
        allowed.Headers.TryAddWithoutValidation("Origin", AllowedTestOrigin);
        var ok = await client.SendAsync(allowed);
        ok.EnsureSuccessStatusCode();
        Assert.True(ok.Headers.TryGetValues("Access-Control-Allow-Origin", out var acao));
        Assert.Equal(AllowedTestOrigin, Assert.Single(acao));

        var denied = new HttpRequestMessage(HttpMethod.Get, "/health");
        denied.Headers.TryAddWithoutValidation("Origin", OtherOrigin);
        var bad = await client.SendAsync(denied);
        bad.EnsureSuccessStatusCode();
        Assert.False(bad.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NonDevelopment_WildcardAllowedOrigins_DoesNotAllowBrowserCors()
    {
        await using var factory = FactoryRestricted("*");
        using var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation("Origin", "https://any.example");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NonDevelopment_Preflight_AllowsOnlyConfiguredHeaders_NotIdempotencyKey()
    {
        await using var factory = FactoryRestricted(AllowedTestOrigin);
        using var client = factory.CreateClient();

        var authPreflight = new HttpRequestMessage(HttpMethod.Options, "/health");
        authPreflight.Headers.TryAddWithoutValidation("Origin", AllowedTestOrigin);
        authPreflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");
        authPreflight.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "authorization");
        var authRes = await client.SendAsync(authPreflight);
        Assert.Equal(HttpStatusCode.NoContent, authRes.StatusCode);
        Assert.True(authRes.Headers.TryGetValues("Access-Control-Allow-Headers", out var authHeaders));
        var joined = string.Join(", ", authHeaders);
        Assert.Contains("Authorization", joined, StringComparison.OrdinalIgnoreCase);

        var idemPreflight = new HttpRequestMessage(HttpMethod.Options, "/health");
        idemPreflight.Headers.TryAddWithoutValidation("Origin", AllowedTestOrigin);
        idemPreflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");
        idemPreflight.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "idempotency-key");
        var idemRes = await client.SendAsync(idemPreflight);
        Assert.Equal(HttpStatusCode.NoContent, idemRes.StatusCode);
        if (idemRes.Headers.TryGetValues("Access-Control-Allow-Headers", out var idemAllowed))
        {
            var idemJoined = string.Join(", ", idemAllowed);
            Assert.DoesNotContain("idempotency-key", idemJoined, StringComparison.OrdinalIgnoreCase);
        }
    }
}
