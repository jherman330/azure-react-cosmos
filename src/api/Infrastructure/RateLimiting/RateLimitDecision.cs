namespace Todo.Api.Infrastructure.RateLimiting;

public readonly struct RateLimitDecision
{
    public RateLimitDecision(bool allowed, int remaining, long resetUnixSeconds, int retryAfterSeconds)
    {
        Allowed = allowed;
        Remaining = remaining;
        ResetUnixSeconds = resetUnixSeconds;
        RetryAfterSeconds = retryAfterSeconds;
    }

    public bool Allowed { get; }
    public int Remaining { get; }
    public long ResetUnixSeconds { get; }
    public int RetryAfterSeconds { get; }
}
