using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Notifications.Commands.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpdateNotificationPreferencesCommand, Result>
{
    public async Task<Result> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await db.NotificationPreferences
            .Where(p => p.UserId == tenantContext.UserId)
            .ToDictionaryAsync(p => p.NotificationType, cancellationToken);

        foreach (var item in request.Preferences)
        {
            var type = (NotificationType)item.NotificationType;
            var config = NotificationTypeConfig.GetConfig(type);

            // Don't allow disabling mandatory channels
            var smsEnabled = config.SmsMandatory || item.SmsEnabled;
            var emailEnabled = config.EmailMandatory || item.EmailEnabled;

            if (existing.TryGetValue(type, out var pref))
            {
                pref.SmsEnabled = smsEnabled;
                pref.EmailEnabled = emailEnabled;
                pref.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newPref = new NotificationPreference(
                    tenantContext.TenantId,
                    tenantContext.UserId,
                    type)
                {
                    SmsEnabled = smsEnabled,
                    EmailEnabled = emailEnabled,
                    UpdatedAt = DateTime.UtcNow,
                };
                db.NotificationPreferences.Add(newPref);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
