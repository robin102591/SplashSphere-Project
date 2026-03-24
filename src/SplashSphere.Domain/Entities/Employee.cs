namespace SplashSphere.Domain.Entities;

/// <summary>
/// A staff member at a specific branch. Compensation is determined by
/// <see cref="EmployeeType"/>:
/// <list type="bullet">
///   <item><see cref="EmployeeType.Commission"/> — earns through service commissions split
///   equally among all employees assigned to a transaction service. <see cref="DailyRate"/> is null.</item>
///   <item><see cref="EmployeeType.Daily"/> — earns a fixed <see cref="DailyRate"/> per day
///   attended. Commissions are not earned.</item>
/// </list>
/// </summary>
public sealed class Employee : IAuditableEntity
{
    private Employee() { } // EF Core

    public Employee(
        string tenantId,
        string branchId,
        string firstName,
        string lastName,
        EmployeeType employeeType,
        decimal? dailyRate = null,
        string? email = null,
        string? contactNumber = null,
        DateOnly? hiredDate = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        FirstName = firstName;
        LastName = lastName;
        EmployeeType = employeeType;
        DailyRate = dailyRate;
        Email = email;
        ContactNumber = contactNumber;
        HiredDate = hiredDate;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
    public EmployeeType EmployeeType { get; set; }

    /// <summary>
    /// Fixed PHP amount paid per day worked. Required for <see cref="EmployeeType.Daily"/>;
    /// null for <see cref="EmployeeType.Commission"/>. Precision (10, 2).
    /// </summary>
    public decimal? DailyRate { get; set; }

    public DateOnly? HiredDate { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional FK linking this employee to their Clerk-backed User account.
    /// Enables PIN management and future self-service features.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// UTC timestamp of the last Clerk organization invitation sent for this employee.
    /// Null if never invited. Once <see cref="UserId"/> is set, the invitation is considered accepted.
    /// </summary>
    public DateTime? InvitedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User? User { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = [];
    public ICollection<PayrollEntry> PayrollEntries { get; set; } = [];
    public ICollection<TransactionEmployee> TransactionSummaries { get; set; } = [];
    public ICollection<ServiceEmployeeAssignment> ServiceAssignments { get; set; } = [];
    public ICollection<PackageEmployeeAssignment> PackageAssignments { get; set; } = [];
}
