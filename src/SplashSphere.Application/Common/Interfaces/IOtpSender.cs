namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Generates a one-time passcode and delivers it to a Philippine mobile number.
/// Used by the Customer Connect app for phone-based authentication (no Clerk).
/// <para>
/// Platform absorbs the SMS cost for OTP delivery — it does NOT decrement the
/// tenant's <c>SmsQuotaMonthly</c>. This keeps the customer sign-up friction flat
/// regardless of which tenant the user first interacts with.
/// </para>
/// </summary>
public interface IOtpSender
{
    /// <summary>
    /// Generate and send an OTP code to <paramref name="phoneNumber"/>.
    /// Returns the generated code so the caller can persist it to the store.
    /// In dev mode (when <c>Otp:FixedCode</c> is configured) the sender returns
    /// that fixed value instead of a random one — but still attempts delivery.
    /// </summary>
    Task<string> SendAsync(string phoneNumber, CancellationToken ct = default);
}
