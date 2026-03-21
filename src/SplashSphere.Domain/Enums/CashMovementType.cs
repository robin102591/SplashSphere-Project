namespace SplashSphere.Domain.Enums;

public enum CashMovementType
{
    CashIn  = 1, // Money added to the drawer (additional change fund, owner deposit)
    CashOut = 2, // Money removed from the drawer (supplies, employee vale, petty cash)
}
