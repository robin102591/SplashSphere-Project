using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// PayMongo payment gateway for the Philippine market.
/// Supports GCash, Maya, credit/debit cards, and bank transfers.
/// <para>
/// API docs: https://developers.paymongo.com/reference
/// Auth: Basic auth with secret key as username (no password).
/// Amounts are in centavos (₱100 = 10000 centavos).
/// </para>
/// </summary>
public sealed class PayMongoPaymentGateway : IPaymentGateway
{
    private const string BaseUrl = "https://api.paymongo.com/v1/";

    private readonly HttpClient _http;
    private readonly string _webhookSecret;
    private readonly ILogger<PayMongoPaymentGateway> _logger;

    public PayMongoPaymentGateway(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PayMongoPaymentGateway> logger)
    {
        _logger = logger;

        var secretKey = configuration["PayMongo:SecretKey"]
            ?? throw new InvalidOperationException("PayMongo:SecretKey is not configured.");
        _webhookSecret = configuration["PayMongo:WebhookSecret"]
            ?? throw new InvalidOperationException("PayMongo:WebhookSecret is not configured.");

        _http = httpClientFactory.CreateClient("PayMongo");
        _http.BaseAddress = new Uri(BaseUrl);

        // PayMongo uses Basic auth: secret key as username, empty password
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<CheckoutSession> CreateCheckoutSessionAsync(
        string tenantId,
        PlanTier targetPlan,
        decimal amount,
        string currency,
        string successUrl,
        string cancelUrl,
        CancellationToken ct,
        string? invoiceId = null)
    {
        var plan = PlanCatalog.GetPlan(targetPlan);

        // PayMongo amounts are in centavos (smallest currency unit)
        var amountInCentavos = (int)(amount * 100);

        var payload = new
        {
            data = new
            {
                attributes = new
                {
                    description = $"SplashSphere {plan.Name} Plan — Monthly Subscription",
                    send_email_receipt = true,
                    show_description = true,
                    show_line_items = true,
                    payment_method_types = new[] { "gcash", "grab_pay", "card", "paymaya", "dob", "dob_ubp" },
                    line_items = new[]
                    {
                        new
                        {
                            name = $"SplashSphere {plan.Name} Plan",
                            description = $"Monthly subscription — {plan.Name} plan",
                            amount = amountInCentavos,
                            currency = currency.ToUpperInvariant(),
                            quantity = 1,
                        }
                    },
                    success_url = successUrl,
                    cancel_url = cancelUrl,
                    metadata = BuildMetadata(tenantId, targetPlan, invoiceId)
                }
            }
        };

        // Serialize without naming policy — properties are already snake_case
        var json = JsonSerializer.Serialize(payload);

        _logger.LogDebug("PayMongo checkout request: {Json}", json);

        var response = await _http.PostAsync(
            "checkout_sessions",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct);

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        _logger.LogDebug("PayMongo checkout response: {Status} {Body}",
            response.StatusCode, responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("PayMongo checkout creation failed: {Status} {Body}",
                response.StatusCode, responseBody);
            throw new InvalidOperationException(
                $"PayMongo checkout creation failed: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var data = doc.RootElement.GetProperty("data");
        var sessionId = data.GetProperty("id").GetString()!;
        var checkoutUrl = data.GetProperty("attributes").GetProperty("checkout_url").GetString()!;

        _logger.LogInformation(
            "PayMongo checkout session created: {SessionId} URL: {Url} for tenant {TenantId}, plan {Plan}",
            sessionId, checkoutUrl, tenantId, targetPlan);

        return new CheckoutSession(sessionId, checkoutUrl);
    }

    public Task<WebhookEvent?> ParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct)
    {
        // Verify HMAC-SHA256 signature
        // PayMongo sends: t={timestamp},te={test_signature},li={live_signature}
        if (!VerifySignature(payload, signature))
        {
            _logger.LogWarning("PayMongo webhook signature verification failed.");
            return Task.FromResult<WebhookEvent?>(null);
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var data = doc.RootElement.GetProperty("data");
            var attributes = data.GetProperty("attributes");
            var eventType = attributes.GetProperty("type").GetString() ?? "";

            // The payment data is nested under attributes.data
            var paymentData = attributes.GetProperty("data");
            var paymentAttributes = paymentData.GetProperty("attributes");

            var paymentId = paymentData.GetProperty("id").GetString() ?? "";
            var amountInCentavos = paymentAttributes.GetProperty("amount").GetInt32();
            var amount = amountInCentavos / 100m;
            var currencyValue = paymentAttributes.GetProperty("currency").GetString() ?? "PHP";
            var status = paymentAttributes.GetProperty("status").GetString() ?? "";

            // Extract tenant ID and plan tier from metadata
            string? tenantId = null;
            PlanTier? targetPlan = null;
            string? invoiceId = null;
            string? paymentMethod = null;

            if (paymentAttributes.TryGetProperty("metadata", out var metadata))
            {
                if (metadata.TryGetProperty("tenant_id", out var tid))
                    tenantId = tid.GetString();
                if (metadata.TryGetProperty("plan_tier", out var pt) &&
                    Enum.TryParse<PlanTier>(pt.GetString(), true, out var parsed))
                    targetPlan = parsed;
                if (metadata.TryGetProperty("invoice_id", out var inv))
                    invoiceId = inv.GetString();
            }

            if (paymentAttributes.TryGetProperty("source", out var source) &&
                source.TryGetProperty("type", out var sourceType))
            {
                paymentMethod = sourceType.GetString();
            }

            var succeeded = eventType.Contains("paid") && status == "paid";

            return Task.FromResult<WebhookEvent?>(new WebhookEvent(
                eventType, paymentId, tenantId, targetPlan, invoiceId,
                amount, currencyValue, paymentMethod, succeeded));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse PayMongo webhook payload.");
            return Task.FromResult<WebhookEvent?>(null);
        }
    }

    private bool VerifySignature(string payload, string signatureHeader)
    {
        if (string.IsNullOrEmpty(signatureHeader)) return false;

        // Parse "t={timestamp},te={test_sig},li={live_sig}"
        var parts = signatureHeader.Split(',')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);

        if (!parts.TryGetValue("t", out var timestamp))
            return false;

        // Use live signature if present, otherwise test signature
        parts.TryGetValue("li", out var liveSig);
        parts.TryGetValue("te", out var testSig);
        var expectedSig = !string.IsNullOrEmpty(liveSig) ? liveSig : testSig;
        if (string.IsNullOrEmpty(expectedSig)) return false;

        // Compute HMAC-SHA256(webhook_secret, "{timestamp}.{payload}")
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSig = Convert.ToHexString(computedHash).ToLowerInvariant();

        var match = string.Equals(computedSig, expectedSig, StringComparison.OrdinalIgnoreCase);
        if (!match)
            _logger.LogWarning(
                "PayMongo signature mismatch. Expected: {Expected}, Computed: {Computed}, Timestamp: {Timestamp}",
                expectedSig, computedSig, timestamp);

        return match;
    }

    private static Dictionary<string, string> BuildMetadata(
        string tenantId, PlanTier targetPlan, string? invoiceId)
    {
        var meta = new Dictionary<string, string>
        {
            ["tenant_id"] = tenantId,
            ["plan_tier"] = targetPlan.ToString(),
        };
        if (!string.IsNullOrEmpty(invoiceId))
            meta["invoice_id"] = invoiceId;
        return meta;
    }
}
