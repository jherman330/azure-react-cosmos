using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Todo.Api.Tests.Integration;

/// <summary>
/// Asserts FluentValidation auto-validation and <see cref="Todo.Api.Infrastructure.ApiValidationErrorEnvelope"/> shape—not demo business semantics.
/// </summary>
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class FluentValidationTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private const string SandboxValidateUrl = "/api/v1/sandbox/validate";
    private const string ExpectedSummaryMessage = "One or more validation errors occurred.";

    private readonly HttpClient _client;

    public FluentValidationTests(WebApplicationFactory<global::Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(b => b.UseEnvironment("Testing"))
            .CreateClient();
    }

    [Fact]
    public async Task Pipeline_EmptyField_Produces400_AndFullValidationEnvelope()
    {
        var response = await _client.PostAsJsonAsync(SandboxValidateUrl, new { message = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));

        Assert.Equal("VALIDATION_FAILED", root.GetProperty("errorCode").GetString());
        Assert.Equal(ExpectedSummaryMessage, root.GetProperty("message").GetString());

        Assert.True(root.TryGetProperty("errors", out var errors));
        Assert.Equal(JsonValueKind.Object, errors.ValueKind);
        Assert.True(errors.TryGetProperty("message", out var fieldErrors));
        Assert.Equal(JsonValueKind.Array, fieldErrors.ValueKind);
        Assert.True(fieldErrors.GetArrayLength() > 0);
        Assert.All(fieldErrors.EnumerateArray(), e =>
        {
            Assert.Equal(JsonValueKind.String, e.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(e.GetString()));
        });
    }

    [Fact]
    public async Task Pipeline_MaxLengthRule_Produces400_AndFieldErrors()
    {
        var tooLong = new string('x', 501);
        var response = await _client.PostAsJsonAsync(SandboxValidateUrl, new { message = tooLong });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.Equal("VALIDATION_FAILED", root.GetProperty("errorCode").GetString());
        Assert.True(root.GetProperty("errors").TryGetProperty("message", out var fieldErrors));
        Assert.Contains(fieldErrors.EnumerateArray(), e =>
            e.GetString()?.Contains("500", StringComparison.Ordinal) == true);
    }

    /// <summary>Confirms pipeline does not block the action when transport rules pass (status only).</summary>
    [Fact]
    public async Task Pipeline_PassesThroughToAction_WhenTransportValid()
    {
        var response = await _client.PostAsJsonAsync(SandboxValidateUrl, new { message = "ok" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("VALIDATION_FAILED", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}
