using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// Sends SMS via the Semaphore PH REST API.
/// Docs: https://semaphore.co/docs
/// </summary>
public sealed class SemaphoreSmsService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<SemaphoreSmsService> logger) : ISmsService
{
    private readonly string _apiKey = configuration["Semaphore:ApiKey"]
        ?? throw new InvalidOperationException("Semaphore:ApiKey is not configured.");
    private readonly string _senderName = configuration["Semaphore:SenderName"] ?? "SplashSphere";

    public async Task<bool> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("Semaphore");
        client.BaseAddress ??= new Uri("https://api.semaphore.co");

        var payload = new Dictionary<string, string>
        {
            ["apikey"] = _apiKey,
            ["number"] = message.PhoneNumber,
            ["message"] = message.Body,
            ["sendername"] = _senderName,
        };

        var response = await client.PostAsync(
            "/api/v4/messages",
            new FormUrlEncodedContent(payload),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "Semaphore API error {StatusCode}: {Body}",
                (int)response.StatusCode, body);
            return false;
        }

        logger.LogInformation(
            "SMS sent to {Number}: {BodyPreview}",
            message.PhoneNumber,
            message.Body.Length > 50 ? message.Body[..50] + "..." : message.Body);
        return true;
    }
}
