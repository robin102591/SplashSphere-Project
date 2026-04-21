using System.Globalization;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.Auth.Connect;

/// <summary>
/// <see cref="IOtpStore"/> backed by <see cref="IDistributedCache"/>.
/// Uses the in-memory cache in dev (registered via <c>AddDistributedMemoryCache</c>)
/// and Redis in production (swap the registration — the contract is unchanged).
/// <para>
/// Rate limits enforced:
/// <list type="bullet">
/// <item>One send per phone per 60 seconds (cooldown).</item>
/// <item>Five sends per phone per 24 hours (daily cap).</item>
/// </list>
/// Both counters live next to the code key with the same phone component so
/// rotating/cleaning a number's state is straightforward.
/// </para>
/// </summary>
public sealed class DistributedCacheOtpStore(IDistributedCache cache) : IOtpStore
{
    private const int CooldownSeconds = 60;
    private const int DailySendCap = 5;
    private static readonly TimeSpan DailyWindow = TimeSpan.FromDays(1);

    private static string CodeKey(string phone) => $"otp:code:{phone}";
    private static string CooldownKey(string phone) => $"otp:cooldown:{phone}";
    private static string DailyKey(string phone) => $"otp:daily:{phone}";

    public Task SaveCodeAsync(string phoneNumber, string code, TimeSpan ttl, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        return cache.SetAsync(CodeKey(phoneNumber), Encoding.UTF8.GetBytes(code), options, ct);
    }

    public async Task<string?> GetCodeAsync(string phoneNumber, CancellationToken ct = default)
    {
        var bytes = await cache.GetAsync(CodeKey(phoneNumber), ct);
        return bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }

    public Task DeleteCodeAsync(string phoneNumber, CancellationToken ct = default)
        => cache.RemoveAsync(CodeKey(phoneNumber), ct);

    public async Task<OtpRateLimitResult> TryRegisterSendAsync(string phoneNumber, CancellationToken ct = default)
    {
        // 60-second cooldown since last send
        var cooldown = await cache.GetAsync(CooldownKey(phoneNumber), ct);
        if (cooldown is not null)
        {
            return new OtpRateLimitResult(false, CooldownSeconds, "Please wait before requesting another code.");
        }

        // Daily cap: 5 sends per rolling 24h window
        var dailyBytes = await cache.GetAsync(DailyKey(phoneNumber), ct);
        var count = dailyBytes is null
            ? 0
            : int.Parse(Encoding.UTF8.GetString(dailyBytes), CultureInfo.InvariantCulture);

        if (count >= DailySendCap)
        {
            return new OtpRateLimitResult(false, (int)DailyWindow.TotalSeconds, "Daily OTP limit reached. Try again tomorrow.");
        }

        // Commit: bump daily counter, arm cooldown
        await cache.SetAsync(
            DailyKey(phoneNumber),
            Encoding.UTF8.GetBytes((count + 1).ToString(CultureInfo.InvariantCulture)),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DailyWindow },
            ct);

        await cache.SetAsync(
            CooldownKey(phoneNumber),
            Encoding.UTF8.GetBytes("1"),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CooldownSeconds) },
            ct);

        return new OtpRateLimitResult(true, 0, null);
    }
}
