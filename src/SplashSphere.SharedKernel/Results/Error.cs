namespace SplashSphere.SharedKernel.Results;

/// <summary>
/// Represents a structured error with a machine-readable code and human-readable message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // ── HTTP-semantic factories ──────────────────────────────────────────────

    public static Error NotFound(string resource, string? id = null) =>
        new("NOT_FOUND", id is null
            ? $"{resource} was not found."
            : $"{resource} '{id}' was not found.");

    public static Error Validation(string message) => new("VALIDATION", message);
    public static Error Validation(string code, string message) => new(code, message);
    public static Error Conflict(string message)   => new("CONFLICT", message);
    public static Error Unauthorized(string message) => new("UNAUTHORIZED", message);
    public static Error Forbidden(string message)  => new("FORBIDDEN", message);

    // ── Domain-specific factories ────────────────────────────────────────────

    public static Error Domain(string message) => new("DOMAIN_RULE", message);

    public static Error InsufficientStock(string itemName, int requested, int available) =>
        new("INSUFFICIENT_STOCK",
            $"Insufficient stock for '{itemName}': requested {requested}, available {available}.");

    public static Error InvalidTransactionState(string currentState, string attemptedAction) =>
        new("INVALID_TRANSACTION_STATE",
            $"Cannot '{attemptedAction}' a transaction in '{currentState}' state.");

    public static Error PayrollPeriodLocked(string periodId) =>
        new("PAYROLL_PERIOD_LOCKED",
            $"Payroll period '{periodId}' is locked and cannot be modified.");

    public static Error TenantNotFound(string tenantId) =>
        new("TENANT_NOT_FOUND", $"Tenant '{tenantId}' was not found.");

    public override string ToString() => $"[{Code}] {Message}";
}
