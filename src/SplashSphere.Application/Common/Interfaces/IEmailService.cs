namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstraction over transactional email delivery (Resend for production, mock for dev).
/// </summary>
public interface IEmailService
{
    /// <summary>Send a single transactional email.</summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

/// <summary>A transactional email ready to send.</summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null,
    string? ReplyTo = null);
