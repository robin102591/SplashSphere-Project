namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Persists OTP codes and enforces rate limits for the Customer Connect
/// phone-auth flow. Backed by <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
/// (in-memory for dev, Redis in production).
/// </summary>
public interface IOtpStore
{
    /// <summary>
    /// Persist the most recent OTP code for <paramref name="phoneNumber"/> with
    /// a TTL (usually 5 minutes). Overwrites any existing code.
    /// </summary>
    Task SaveCodeAsync(string phoneNumber, string code, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>
    /// Retrieve the active OTP code for <paramref name="phoneNumber"/>, or null
    /// if none exists or it has expired.
    /// </summary>
    Task<string?> GetCodeAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>Remove the active OTP code for <paramref name="phoneNumber"/> (consume-on-verify).</summary>
    Task DeleteCodeAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Record a send attempt. Returns <c>false</c> if the rate limit has been
    /// exceeded — either the 60-second cooldown or the 5-send-per-day cap.
    /// The caller should respond with 429 + Retry-After.
    /// </summary>
    Task<OtpRateLimitResult> TryRegisterSendAsync(string phoneNumber, CancellationToken ct = default);
}

/// <summary>Result of a rate-limit check for OTP sends.</summary>
public sealed record OtpRateLimitResult(bool Allowed, int RetryAfterSeconds, string? Reason);
