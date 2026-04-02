using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// Development mock that logs emails instead of sending them.
/// Used when Resend:ApiKey is not configured.
/// </summary>
public sealed class MockEmailService(
    ILogger<MockEmailService> logger) : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[MockEmail] To: {To} | Subject: {Subject} | Body length: {Length}",
            message.To, message.Subject, message.HtmlBody?.Length ?? 0);
        return Task.CompletedTask;
    }
}
