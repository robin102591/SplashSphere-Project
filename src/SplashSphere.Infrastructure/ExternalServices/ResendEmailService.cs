using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// Sends transactional emails via the Resend REST API.
/// Docs: https://resend.com/docs/api-reference/emails/send-email
/// </summary>
public sealed class ResendEmailService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ResendEmailService> logger) : IEmailService
{
    private readonly string _apiKey = configuration["Resend:ApiKey"]
        ?? throw new InvalidOperationException("Resend:ApiKey is not configured.");
    private readonly string _fromEmail = configuration["Resend:FromEmail"] ?? "SplashSphere <noreply@splashsphere.ph>";

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("Resend");
        client.BaseAddress ??= new Uri("https://api.resend.com");
        client.DefaultRequestHeaders.Authorization = new("Bearer", _apiKey);

        var payload = new
        {
            from = _fromEmail,
            to = new[] { message.To },
            subject = message.Subject,
            html = message.HtmlBody,
            text = message.TextBody,
            reply_to = message.ReplyTo,
        };

        var response = await client.PostAsJsonAsync("/emails", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "Resend API error {StatusCode}: {Body}",
                (int)response.StatusCode, body);
            return;
        }

        logger.LogInformation(
            "Email sent to {To}: {Subject}", message.To, message.Subject);
    }
}
