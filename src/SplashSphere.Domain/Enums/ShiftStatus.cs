namespace SplashSphere.Domain.Enums;

public enum ShiftStatus
{
    Open   = 1, // Shift is active; cashier is operating
    Closed = 2, // Cashier has counted the drawer and submitted
    Voided = 3, // Shift was voided (opened by mistake; no transactions processed)
}
