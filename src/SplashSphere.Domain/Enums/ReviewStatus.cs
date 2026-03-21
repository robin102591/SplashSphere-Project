namespace SplashSphere.Domain.Enums;

public enum ReviewStatus
{
    Pending  = 1, // Not yet reviewed by a manager
    Approved = 2, // Manager accepted the variance
    Flagged  = 3, // Manager flagged for investigation (variance too large or suspicious)
}
