namespace SplashSphere.Domain.Enums;

/// <summary>
/// Determines how an employee is compensated.
/// </summary>
public enum EmployeeType
{
    /// <summary>
    /// Earns purely through service commissions. No fixed daily rate.
    /// Commission is split equally among all employees assigned to a service.
    /// </summary>
    Commission = 1,

    /// <summary>
    /// Earns a fixed amount per day worked (e.g. cashiers, security, maintenance).
    /// Daily rate is tallied per attendance record in the payroll period.
    /// </summary>
    Daily = 2,

    /// <summary>
    /// Earns both a fixed daily rate AND service commissions.
    /// Base salary = dailyRate × daysWorked, plus commission splits from assigned services.
    /// </summary>
    Hybrid = 3,
}
