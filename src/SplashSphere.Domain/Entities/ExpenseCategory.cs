namespace SplashSphere.Domain.Entities;

public sealed class ExpenseCategory : IAuditableEntity, ITenantScoped
{
    private ExpenseCategory() { }

    public ExpenseCategory(string tenantId, string name, string? icon = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
        Icon = icon;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = [];
}
