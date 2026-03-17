namespace Todo.Api.Tests;

/// <summary>
/// Test categorization traits for filtering (AC-FOUNDATION-012.4).
/// FastLocal: fast tests suitable for local development; FullCI: all tests including integration, run in CI.
/// </summary>
public static class TestTraits
{
    public const string Category = "Category";
    public const string FastLocal = "FastLocal";
    public const string FullCI = "FullCI";
}
