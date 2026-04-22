using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Notifications;

/// <summary>
/// Routing configuration for a single notification type.
/// Determines which channels are available and whether they're mandatory.
/// </summary>
public sealed record NotificationChannelConfig
{
    /// <summary>The category this notification belongs to.</summary>
    public NotificationCategory Category { get; init; }

    /// <summary>Default severity when no override is specified.</summary>
    public NotificationSeverity DefaultSeverity { get; init; }

    /// <summary>In-app is always delivered — no toggle.</summary>
    public bool InApp { get; init; } = true;

    /// <summary>SMS can be sent for this type.</summary>
    public bool SmsAvailable { get; init; }

    /// <summary>SMS is mandatory (cannot be disabled by user preference).</summary>
    public bool SmsMandatory { get; init; }

    /// <summary>Email can be sent for this type.</summary>
    public bool EmailAvailable { get; init; }

    /// <summary>Email is mandatory (cannot be disabled — e.g., billing emails).</summary>
    public bool EmailMandatory { get; init; }

    /// <summary>If true, SMS targets the customer phone (not the tenant user).</summary>
    public bool SmsToCustomer { get; init; }
}

/// <summary>
/// Static registry of notification routing rules per type.
/// Single source of truth for multi-channel notification delivery.
/// </summary>
public static class NotificationTypeConfig
{
    private static readonly Dictionary<NotificationType, NotificationChannelConfig> Configs = new()
    {
        // -- Operations -------------------------------------------------------
        [NotificationType.TransactionCompleted] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Info,
        },
        [NotificationType.TransactionVoided] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Warning,
            SmsAvailable = true,
        },
        [NotificationType.ShiftClosed] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Info,
            SmsAvailable = true,
        },
        [NotificationType.ShiftFlagged] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Critical,
            SmsAvailable = true,
            SmsMandatory = true,
        },
        [NotificationType.QueueCustomerCalled] = new()
        {
            Category = NotificationCategory.Queue,
            DefaultSeverity = NotificationSeverity.Info,
        },
        [NotificationType.QueueNoShow] = new()
        {
            Category = NotificationCategory.Queue,
            DefaultSeverity = NotificationSeverity.Warning,
        },
        [NotificationType.EmployeeClockedIn] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Info,
        },

        // -- Inventory --------------------------------------------------------
        [NotificationType.LowStockAlert] = new()
        {
            Category = NotificationCategory.Inventory,
            DefaultSeverity = NotificationSeverity.Warning,
            SmsAvailable = true,
        },
        [NotificationType.OutOfStock] = new()
        {
            Category = NotificationCategory.Inventory,
            DefaultSeverity = NotificationSeverity.Critical,
            SmsAvailable = true,
            SmsMandatory = true,
        },

        // -- Financial --------------------------------------------------------
        [NotificationType.PayrollProcessed] = new()
        {
            Category = NotificationCategory.Finance,
            DefaultSeverity = NotificationSeverity.Info,
            EmailAvailable = true,
        },
        [NotificationType.PayrollReadyForReview] = new()
        {
            Category = NotificationCategory.Finance,
            DefaultSeverity = NotificationSeverity.Info,
            SmsAvailable = true,
        },
        [NotificationType.PayrollClosed] = new()
        {
            Category = NotificationCategory.Finance,
            DefaultSeverity = NotificationSeverity.Info,
        },
        [NotificationType.CashAdvanceRequested] = new()
        {
            Category = NotificationCategory.Finance,
            DefaultSeverity = NotificationSeverity.Info,
            SmsAvailable = true,
        },
        [NotificationType.CashAdvanceApproved] = new()
        {
            Category = NotificationCategory.Finance,
            DefaultSeverity = NotificationSeverity.Info,
            SmsAvailable = true,
        },

        // -- Billing (email always mandatory) ---------------------------------
        [NotificationType.BillingInvoiceCreated] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Info,
            EmailAvailable = true,
            EmailMandatory = true,
        },
        [NotificationType.BillingPaymentReminder] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Warning,
            SmsAvailable = true,
            SmsMandatory = true,
            EmailAvailable = true,
            EmailMandatory = true,
        },
        [NotificationType.BillingPaymentReceived] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Info,
            EmailAvailable = true,
            EmailMandatory = true,
        },
        [NotificationType.BillingPaymentFailed] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Critical,
            SmsAvailable = true,
            SmsMandatory = true,
            EmailAvailable = true,
            EmailMandatory = true,
        },
        [NotificationType.BillingOverdue] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Critical,
            SmsAvailable = true,
            SmsMandatory = true,
            EmailAvailable = true,
            EmailMandatory = true,
        },
        [NotificationType.BillingSuspended] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Critical,
            SmsAvailable = true,
            SmsMandatory = true,
            EmailAvailable = true,
            EmailMandatory = true,
        },
        [NotificationType.BillingTrialExpiring] = new()
        {
            Category = NotificationCategory.Billing,
            DefaultSeverity = NotificationSeverity.Warning,
            SmsAvailable = true,
            SmsMandatory = true,
            EmailAvailable = true,
            EmailMandatory = true,
        },

        // -- Customer (SMS to customer phone, not tenant user) ----------------
        [NotificationType.CustomerCarReady] = new()
        {
            Category = NotificationCategory.Customer,
            DefaultSeverity = NotificationSeverity.Info,
            SmsAvailable = true,
            SmsToCustomer = true,
            InApp = false,
        },
        [NotificationType.CustomerLoyaltyTierUp] = new()
        {
            Category = NotificationCategory.Customer,
            DefaultSeverity = NotificationSeverity.Info,
            SmsAvailable = true,
            SmsToCustomer = true,
            InApp = false,
        },

        // -- Platform ---------------------------------------------------------
        [NotificationType.PlatformAnnouncement] = new()
        {
            Category = NotificationCategory.Platform,
            DefaultSeverity = NotificationSeverity.Info,
            EmailAvailable = true,
        },

        // -- Bookings (Customer Connect) --------------------------------------
        [NotificationType.BookingConfirmed] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Info,
        },
        [NotificationType.BookingNoShow] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Warning,
        },
        [NotificationType.BookingReminderSent] = new()
        {
            Category = NotificationCategory.Operations,
            DefaultSeverity = NotificationSeverity.Info,
        },

        // -- Queue position changes (Customer-facing, poll-friendly) ----------
        [NotificationType.QueuePositionChanged] = new()
        {
            Category = NotificationCategory.Queue,
            DefaultSeverity = NotificationSeverity.Info,
        },

        // -- Referrals --------------------------------------------------------
        [NotificationType.ReferralCompleted] = new()
        {
            Category = NotificationCategory.Customer,
            DefaultSeverity = NotificationSeverity.Info,
        },
    };

    /// <summary>Get the channel config for a notification type.</summary>
    public static NotificationChannelConfig GetConfig(NotificationType type)
        => Configs.TryGetValue(type, out var config)
            ? config
            : new NotificationChannelConfig
            {
                Category = NotificationCategory.Operations,
                DefaultSeverity = NotificationSeverity.Info,
            };

    /// <summary>Get all configurable notification types for the preferences UI.</summary>
    public static IReadOnlyDictionary<NotificationType, NotificationChannelConfig> GetAll() => Configs;
}
