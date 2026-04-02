using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// Development mock that logs SMS instead of sending them.
/// Used when Semaphore:ApiKey is not configured.
/// </summary>
public sealed class MockSmsService(
    ILogger<MockSmsService> logger) : ISmsService
{
    public Task<bool> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[MockSMS] To: {Number} | Body: {Body}",
            message.PhoneNumber, message.Body);
        return Task.FromResult(true);
    }
}
