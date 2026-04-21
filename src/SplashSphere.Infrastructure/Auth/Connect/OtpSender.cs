using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.Auth.Connect;

/// <summary>
/// Generates a numeric OTP and dispatches it via the existing <see cref="ISmsService"/>.
/// <para>
/// <b>Dev-mode bypass:</b> when <c>Otp:FixedCode</c> is configured (e.g., "123456"),
/// the sender returns that fixed value instead of a cryptographically random one.
/// The SMS is still attempted — set the SMS service to the Mock implementation to
/// avoid real API calls in local development.
/// </para>
/// <para>
/// Platform absorbs the OTP SMS cost — it does NOT decrement the tenant's
/// <c>SmsQuotaMonthly</c> because the customer is not yet linked to a tenant at
/// send time and making sign-up cost-sensitive would create perverse incentives.
/// </para>
/// </summary>
public sealed class OtpSender(
    ISmsService smsService,
    IConfiguration configuration,
    ILogger<OtpSender> logger) : IOtpSender
{
    private readonly string? _fixedCode = configuration["Otp:FixedCode"];
    private readonly int _codeLength = int.TryParse(configuration["Otp:CodeLength"], out var len) ? len : 6;

    public async Task<string> SendAsync(string phoneNumber, CancellationToken ct = default)
    {
        var code = !string.IsNullOrWhiteSpace(_fixedCode)
            ? _fixedCode
            : GenerateCode(_codeLength);

        var body = $"Your SplashSphere code is {code}. It expires in 5 minutes.";
        var sent = await smsService.SendAsync(new SmsMessage(phoneNumber, body), ct);

        if (!sent)
        {
            logger.LogWarning("OTP SMS delivery failed for {Phone} — caller should retry", phoneNumber);
        }

        return code;
    }

    private static string GenerateCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(max);
        return value.ToString($"D{length}");
    }
}
