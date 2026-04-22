namespace SplashSphere.Domain.Enums;

public enum NotificationType
{
    // Operations
    TransactionCompleted  = 1,
    TransactionVoided     = 2,
    ShiftClosed           = 3,
    ShiftFlagged          = 4,
    QueueCustomerCalled   = 5,
    QueueNoShow           = 6,
    EmployeeClockedIn     = 7,

    // Inventory
    LowStockAlert = 10,
    OutOfStock    = 11,

    // Financial
    PayrollProcessed      = 20,
    PayrollReadyForReview = 21,
    PayrollClosed         = 22,
    CashAdvanceRequested  = 23,
    CashAdvanceApproved   = 24,

    // Billing
    BillingInvoiceCreated  = 30,
    BillingPaymentReminder = 31,
    BillingPaymentReceived = 32,
    BillingPaymentFailed   = 33,
    BillingOverdue         = 34,
    BillingSuspended       = 35,
    BillingTrialExpiring   = 36,

    // Customer (SMS to customer phone)
    CustomerCarReady       = 40,
    CustomerLoyaltyTierUp  = 41,

    // Platform
    PlatformAnnouncement = 50,

    // Bookings (Customer Connect — admin/POS visibility)
    BookingConfirmed     = 60,
    BookingNoShow        = 61,
    BookingReminderSent  = 62,

    // Queue (Customer Connect — position changes)
    QueuePositionChanged = 70,

    // Referrals (Customer Connect)
    ReferralCompleted    = 80,
}
