using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records a manual cash-in or cash-out during an open <see cref="CashierShift"/>.
/// <para>
/// <b>Examples:</b><br/>
/// CashOut — "Soap &amp; chemicals purchase", "Employee meals", "Juan D.C. vale"<br/>
/// CashIn  — "Additional change from owner", "Owner deposit"
/// </para>
/// <para>
/// Movements are <b>append-only</b> during the shift. To correct a mistake,
/// add a reversing entry (e.g. CashIn ₱500 to undo an erroneous CashOut ₱500).
/// </para>
/// Normal customer payments from completed transactions are NOT recorded here —
/// they are read from the <c>Payment</c> table when the shift is closed.
/// </summary>
public sealed class CashMovement : IAuditableEntity, ITenantScoped
{
    private CashMovement() { } // EF Core

    public CashMovement(
        string tenantId,
        string cashierShiftId,
        CashMovementType type,
        decimal amount,
        string reason,
        string? reference,
        DateTime movementTime)
    {
        Id             = Guid.NewGuid().ToString();
        TenantId       = tenantId;
        CashierShiftId = cashierShiftId;
        Type           = type;
        Amount         = amount;
        Reason         = reason;
        Reference      = reference;
        MovementTime   = movementTime;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CashierShiftId { get; set; } = string.Empty;

    public CashMovementType Type { get; set; }

    /// <summary>Amount moved in/out of the drawer (₱). Must be greater than zero.</summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Human-readable reason for the movement.
    /// Examples: "Soap purchase", "Employee meals", "Employee vale", "Additional change from owner".
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional supporting reference: receipt number, employee name, etc.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>UTC timestamp when the cash physically moved.</summary>
    public DateTime MovementTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public CashierShift CashierShift { get; set; } = null!;
}
