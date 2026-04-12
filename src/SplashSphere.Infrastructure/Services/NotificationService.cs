using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Notifications;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Infrastructure.Hubs;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Unified notification service. Creates a persistent notification record,
/// broadcasts in-app via SignalR, and routes SMS/email delivery based on
/// notification type configuration and user preferences.
/// </summary>
public sealed class NotificationService(
    IApplicationDbContext db,
    IHubContext<SplashSphereHub> hub,
    ISmsService smsService,
    IEmailService emailService,
    IPlanEnforcementService planEnforcement,
    ILogger<NotificationService> logger) : INotificationService
{
    /// <summary>Backward-compatible overload used by existing event handlers.</summary>
    public Task CreateAsync(
        string tenantId,
        NotificationType type,
        NotificationCategory category,
        string title,
        string message,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(new SendNotificationRequest
        {
            TenantId = tenantId,
            Type = type,
            Title = title,
            Message = message,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
        }, cancellationToken);
    }

    public async Task SendAsync(SendNotificationRequest request, CancellationToken ct = default)
    {
        var config = NotificationTypeConfig.GetConfig(request.Type);

        var notification = new Notification(
            request.TenantId,
            request.Type,
            config.Category,
            request.Title,
            request.Message,
            request.ReferenceId,
            request.ReferenceType,
            config.DefaultSeverity,
            request.RecipientUserId,
            request.ActionUrl,
            request.ActionLabel)
        {
            RecipientPhone = request.RecipientPhone,
            RecipientEmail = request.RecipientEmail,
            Metadata = request.Metadata,
        };

        db.Notifications.Add(notification);

        // ── In-App (SignalR) ────────────────────────────────────────────────
        if (config.InApp)
        {
            notification.InAppDelivered = true;
            await hub.Clients
                .Group(SplashSphereHub.TenantGroup(request.TenantId))
                .SendAsync("NotificationReceived", new NotificationReceivedPayload(
                    notification.Id,
                    (int)notification.Type,
                    (int)notification.Category,
                    (int)notification.Severity,
                    notification.Title,
                    notification.Message,
                    notification.ReferenceId,
                    notification.ReferenceType,
                    notification.ActionUrl,
                    notification.ActionLabel,
                    notification.CreatedAt),
                    ct);
        }

        // ── SMS ─────────────────────────────────────────────────────────────
        if (config.SmsAvailable)
        {
            await TrySendSmsAsync(notification, config, request, ct);
        }

        // ── Email ───────────────────────────────────────────────────────────
        if (config.EmailAvailable)
        {
            await TrySendEmailAsync(notification, config, request, ct);
        }
    }

    private async Task TrySendSmsAsync(
        Notification notification,
        NotificationChannelConfig config,
        SendNotificationRequest request,
        CancellationToken ct)
    {
        try
        {
            // Determine phone number
            var phone = config.SmsToCustomer
                ? request.RecipientPhone
                : await ResolveUserPhoneAsync(request.TenantId, request.RecipientUserId, ct);

            if (string.IsNullOrWhiteSpace(phone))
            {
                notification.SmsSkipped = true;
                return;
            }

            // Check if mandatory or user opted in
            var shouldSend = config.SmsMandatory
                || config.DefaultSeverity == NotificationSeverity.Critical
                || await IsChannelEnabledAsync(request.TenantId, request.RecipientUserId, request.Type, "sms", ct);

            if (!shouldSend)
            {
                notification.SmsSkipped = true;
                return;
            }

            // Check SMS quota (skip for customer-facing — those are part of the service)
            if (!config.SmsToCustomer)
            {
                var hasQuota = await planEnforcement.HasSmsQuotaAsync(request.TenantId, ct);
                if (!hasQuota)
                {
                    notification.SmsSkipped = true;
                    logger.LogInformation("SMS skipped for {Type} — quota exhausted for tenant {TenantId}.",
                        request.Type, request.TenantId);
                    return;
                }
            }

            var body = $"[SplashSphere] {notification.Message}";
            if (body.Length > 160) body = body[..157] + "...";

            var sent = await smsService.SendAsync(new SmsMessage(phone, body), ct);
            notification.SmsDelivered = sent;
            if (!sent) notification.SmsSkipped = true;

            if (sent && !config.SmsToCustomer)
            {
                await planEnforcement.IncrementSmsUsageAsync(request.TenantId, ct);
            }
        }
        catch (Exception ex)
        {
            notification.SmsSkipped = true;
            logger.LogWarning(ex, "SMS delivery failed for notification {Type}.", request.Type);
        }
    }

    private async Task TrySendEmailAsync(
        Notification notification,
        NotificationChannelConfig config,
        SendNotificationRequest request,
        CancellationToken ct)
    {
        try
        {
            var email = request.RecipientEmail
                ?? await ResolveUserEmailAsync(request.TenantId, request.RecipientUserId, ct);

            if (string.IsNullOrWhiteSpace(email))
            {
                notification.EmailSkipped = true;
                return;
            }

            var shouldSend = config.EmailMandatory
                || await IsChannelEnabledAsync(request.TenantId, request.RecipientUserId, request.Type, "email", ct);

            if (!shouldSend)
            {
                notification.EmailSkipped = true;
                return;
            }

            await emailService.SendAsync(new EmailMessage(
                To: email,
                Subject: notification.Title,
                HtmlBody: BuildEmailHtml(notification)), ct);

            notification.EmailDelivered = true;
        }
        catch (Exception ex)
        {
            notification.EmailSkipped = true;
            logger.LogWarning(ex, "Email delivery failed for notification {Type}.", request.Type);
        }
    }

    private async Task<bool> IsChannelEnabledAsync(
        string tenantId, string? userId, NotificationType type, string channel, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        var pref = await db.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == type, ct);

        return channel switch
        {
            "sms" => pref?.SmsEnabled ?? false,
            "email" => pref?.EmailEnabled ?? false,
            _ => false
        };
    }

    private async Task<string?> ResolveUserPhoneAsync(string tenantId, string? userId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(userId)) return null;

        // Look up contact from the Employee entity linked to this user
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => e.ContactNumber)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<string?> ResolveUserEmailAsync(string tenantId, string? userId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(userId)) return null;

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);
    }

    private static string BuildEmailHtml(Notification n)
    {
        var severityColor = n.Severity switch
        {
            NotificationSeverity.Critical => "#dc2626",
            NotificationSeverity.Warning => "#d97706",
            _ => "#2563eb"
        };

        var actionHtml = !string.IsNullOrEmpty(n.ActionUrl)
            ? $"""<p style="margin-top:16px"><a href="{n.ActionUrl}" style="background:{severityColor};color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;font-weight:600">{n.ActionLabel ?? "View Details"}</a></p>"""
            : "";

        return $"""
            <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;max-width:600px;margin:0 auto;padding:24px">
                <div style="border-left:4px solid {severityColor};padding:16px;background:#f9fafb;border-radius:0 8px 8px 0">
                    <h2 style="margin:0 0 8px;color:#111827">{n.Title}</h2>
                    <p style="margin:0;color:#4b5563;line-height:1.6">{n.Message}</p>
                    {actionHtml}
                </div>
                <p style="margin-top:24px;font-size:12px;color:#9ca3af">SplashSphere — Car Wash Management</p>
            </div>
            """;
    }
}
