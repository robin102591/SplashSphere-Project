using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

public sealed class Expense : IAuditableEntity
{
    private Expense() { }

    public Expense(
        string tenantId,
        string branchId,
        string recordedById,
        string categoryId,
        decimal amount,
        string description,
        DateTime expenseDate)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        RecordedById = recordedById;
        CategoryId = categoryId;
        Amount = amount;
        Description = description;
        ExpenseDate = expenseDate;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string RecordedById { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? ReceiptReference { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseFrequency Frequency { get; set; } = ExpenseFrequency.OneTime;
    public bool IsRecurring { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
}
