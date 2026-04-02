namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstraction over SMS delivery (Semaphore for PH production, mock for dev).
/// </summary>
public interface ISmsService
{
    /// <summary>Send an SMS to a Philippine mobile number.</summary>
    Task<bool> SendAsync(SmsMessage message, CancellationToken ct = default);
}

/// <summary>An SMS message ready to send.</summary>
public sealed record SmsMessage(
    string PhoneNumber,
    string Body);
