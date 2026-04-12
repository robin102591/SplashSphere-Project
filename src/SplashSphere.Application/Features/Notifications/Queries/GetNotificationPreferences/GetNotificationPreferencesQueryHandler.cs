using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Notifications.Queries.GetNotificationPreferences;

public sealed class GetNotificationPreferencesQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetNotificationPreferencesQuery, List<NotificationPreferenceDto>>
{
    public async Task<List<NotificationPreferenceDto>> Handle(
        GetNotificationPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        // Get all saved preferences for this user
        var saved = await db.NotificationPreferences
            .AsNoTracking()
            .Where(p => p.UserId == tenantContext.UserId)
            .ToDictionaryAsync(p => p.NotificationType, cancellationToken);

        // Build the full list from the config registry
        var result = new List<NotificationPreferenceDto>();

        foreach (var (type, config) in NotificationTypeConfig.GetAll())
        {
            // Skip customer-facing types (not user-configurable)
            if (config.SmsToCustomer) continue;
            // Skip types with no configurable channels
            if (!config.SmsAvailable && !config.EmailAvailable) continue;

            saved.TryGetValue(type, out var pref);

            result.Add(new NotificationPreferenceDto(
                NotificationType: (int)type,
                TypeName: type.ToString(),
                Category: (int)config.Category,
                SmsAvailable: config.SmsAvailable,
                SmsMandatory: config.SmsMandatory,
                EmailAvailable: config.EmailAvailable,
                EmailMandatory: config.EmailMandatory,
                SmsEnabled: config.SmsMandatory || (pref?.SmsEnabled ?? false),
                EmailEnabled: config.EmailMandatory || (pref?.EmailEnabled ?? false)));
        }

        return result;
    }
}
