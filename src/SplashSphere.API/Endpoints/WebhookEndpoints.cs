using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;

namespace SplashSphere.API.Endpoints;

/// <summary>
/// Receives Clerk webhook events and keeps the internal database in sync.
/// <para>
/// Events handled:
/// <list type="bullet">
///   <item><c>user.created</c> — creates an internal <see cref="User"/> row.</item>
///   <item><c>organizationMembership.created</c> — links the user to the tenant
///   (sets <see cref="User.TenantId"/> and <see cref="User.Role"/>).</item>
/// </list>
/// </para>
/// Requests are verified using the Svix HMAC-SHA256 signature scheme.
/// Configure <c>Clerk:WebhookSecret</c> (the "whsec_..." value from the Clerk dashboard).
/// </summary>
public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        // POST /webhooks/clerk — no auth, no tenant resolution
        app.MapPost("/webhooks/clerk", async (
            HttpRequest request,
            IApplicationDbContext db,
            IConfiguration configuration,
            ILogger<WebhookMarker> logger,
            CancellationToken ct) =>
        {
            // ── 1. Read raw body ──────────────────────────────────────────────
            using var reader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync(ct);

            // ── 2. Verify Svix signature ──────────────────────────────────────
            var secret = configuration["Clerk:WebhookSecret"];
            if (!string.IsNullOrEmpty(secret))
            {
                if (!VerifySvixSignature(request.Headers, body, secret, logger))
                    return Results.Unauthorized();
            }

            // ── 3. Parse payload ──────────────────────────────────────────────
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Clerk webhook: invalid JSON body.");
                return Results.BadRequest();
            }

            using (doc)
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeEl))
                    return Results.Ok(); // unknown shape — ack anyway

                var eventType = typeEl.GetString();
                var data = root.TryGetProperty("data", out var dataEl) ? dataEl : default;

                switch (eventType)
                {
                    case "user.created":
                        await HandleUserCreated(data, db, logger, ct);
                        break;

                    case "organizationMembership.created":
                        await HandleMembershipCreated(data, db, logger, ct);
                        break;

                    default:
                        logger.LogDebug("Clerk webhook: unhandled event type {EventType}.", eventType);
                        break;
                }
            }

            return Results.Ok();
        })
        .WithName("ClerkWebhook")
        .WithTags("Webhooks")
        .WithSummary("Receives Clerk webhook events (user.created, organizationMembership.created).");

        return app;
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an internal User row for a newly signed-up Clerk user.
    /// Idempotent — skips creation if a user with the same ClerkUserId already exists.
    /// </summary>
    private static async Task HandleUserCreated(
        JsonElement data,
        IApplicationDbContext db,
        ILogger logger,
        CancellationToken ct)
    {
        var clerkUserId  = GetString(data, "id");
        var email        = GetFirstEmailAddress(data);
        var firstName    = GetString(data, "first_name");
        var lastName     = GetString(data, "last_name");

        if (string.IsNullOrEmpty(clerkUserId) || string.IsNullOrEmpty(email))
        {
            logger.LogWarning("Clerk webhook user.created: missing id or email.");
            return;
        }

        // Idempotency — skip if user already synced (e.g. webhook replay).
        var exists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.ClerkUserId == clerkUserId, ct);

        if (exists)
        {
            logger.LogDebug("Clerk webhook user.created: user {ClerkUserId} already exists — skipping.", clerkUserId);
            return;
        }

        var user = new User(clerkUserId, email, firstName ?? string.Empty, lastName ?? string.Empty);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Clerk webhook user.created: created internal user for {ClerkUserId}.", clerkUserId);
    }

    /// <summary>
    /// Links an existing User to a Tenant when they accept an organization invitation.
    /// Sets <see cref="User.TenantId"/> and <see cref="User.Role"/>.
    /// Skips silently if either the user or organization is not found in the database
    /// (can happen if the tenant was created via onboarding before the webhook fired).
    /// </summary>
    private static async Task HandleMembershipCreated(
        JsonElement data,
        IApplicationDbContext db,
        ILogger logger,
        CancellationToken ct)
    {
        // data.public_user_data.user_id
        var clerkUserId = string.Empty;
        if (data.TryGetProperty("public_user_data", out var pub) &&
            pub.TryGetProperty("user_id", out var uid))
        {
            clerkUserId = uid.GetString() ?? string.Empty;
        }

        // data.organization.id
        var orgId = string.Empty;
        if (data.TryGetProperty("organization", out var org) &&
            org.TryGetProperty("id", out var oid))
        {
            orgId = oid.GetString() ?? string.Empty;
        }

        // data.role  e.g. "org:admin" / "org:member"
        var role = GetString(data, "role");

        if (string.IsNullOrEmpty(clerkUserId) || string.IsNullOrEmpty(orgId))
        {
            logger.LogWarning("Clerk webhook organizationMembership.created: missing user_id or org id.");
            return;
        }

        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId, ct);

        if (user is null)
        {
            logger.LogWarning(
                "Clerk webhook organizationMembership.created: no internal user for {ClerkUserId}.", clerkUserId);
            return;
        }

        // Only update if not already linked to avoid overwriting an onboarding-flow link.
        if (!string.IsNullOrEmpty(user.TenantId))
        {
            logger.LogDebug(
                "Clerk webhook organizationMembership.created: user {ClerkUserId} already linked to tenant {TenantId} — skipping.",
                clerkUserId, user.TenantId);
            return;
        }

        user.TenantId = orgId;
        user.Role     = role;

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Clerk webhook organizationMembership.created: linked user {ClerkUserId} to tenant {OrgId}.",
            clerkUserId, orgId);
    }

    // ── Svix signature verification ──────────────────────────────────────────

    /// <summary>
    /// Verifies the Svix HMAC-SHA256 webhook signature.
    /// Svix signs: <c>"{msgId}.{timestamp}.{body}"</c> using the webhook secret
    /// (base64-decoded after stripping the <c>whsec_</c> prefix).
    /// </summary>
    private static bool VerifySvixSignature(
        IHeaderDictionary headers,
        string body,
        string secret,
        ILogger logger)
    {
        var msgId    = headers["svix-id"].FirstOrDefault();
        var timestamp = headers["svix-timestamp"].FirstOrDefault();
        var signatures = headers["svix-signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(msgId) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signatures))
        {
            logger.LogWarning("Clerk webhook: missing Svix headers.");
            return false;
        }

        // Reject timestamps older than 5 minutes to prevent replay attacks.
        if (long.TryParse(timestamp, out var ts))
        {
            var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ts;
            if (age > 300 || age < -300)
            {
                logger.LogWarning("Clerk webhook: timestamp too old or too far in the future ({Age}s).", age);
                return false;
            }
        }

        // Decode the secret (strip optional "whsec_" prefix then base64-decode).
        var rawSecret = secret.StartsWith("whsec_", StringComparison.OrdinalIgnoreCase)
            ? secret["whsec_".Length..]
            : secret;

        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(rawSecret);
        }
        catch
        {
            logger.LogWarning("Clerk webhook: invalid WebhookSecret encoding.");
            return false;
        }

        var toSign = $"{msgId}.{timestamp}.{body}";
        var computed = Convert.ToBase64String(
            HMACSHA256.HashData(keyBytes, Encoding.UTF8.GetBytes(toSign)));
        var expected = $"v1,{computed}";

        // signatures header may contain multiple comma/space-separated values.
        var valid = signatures
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(s => string.Equals(s.Trim(), expected, StringComparison.Ordinal));

        if (!valid)
            logger.LogWarning("Clerk webhook: signature mismatch.");

        return valid;
    }

    // ── JSON helpers ─────────────────────────────────────────────────────────

    private static string? GetString(JsonElement el, string property)
        => el.TryGetProperty(property, out var v) ? v.GetString() : null;

    /// <summary>
    /// Extracts the primary email address from the <c>email_addresses</c> array.
    /// Falls back to the first entry if no primary is flagged.
    /// </summary>
    private static string GetFirstEmailAddress(JsonElement data)
    {
        if (!data.TryGetProperty("email_addresses", out var emails))
            return string.Empty;

        // Try the primary first.
        var primaryId = GetString(data, "primary_email_address_id");
        foreach (var entry in emails.EnumerateArray())
        {
            if (primaryId is not null &&
                GetString(entry, "id") == primaryId)
            {
                return GetString(entry, "email_address") ?? string.Empty;
            }
        }

        // Fallback to first.
        foreach (var entry in emails.EnumerateArray())
            return GetString(entry, "email_address") ?? string.Empty;

        return string.Empty;
    }

    // Marker type for ILogger<T> generic parameter.
    private sealed class WebhookMarker;
}
