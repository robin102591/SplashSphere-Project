namespace SplashSphere.SharedKernel.Exceptions;

// ── Base ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Root exception for all application-specific exceptions.
/// Middleware maps subclasses to appropriate HTTP status codes.
/// </summary>
public abstract class SplashSphereException(string message, string? errorCode = null, Exception? inner = null)
    : Exception(message, inner)
{
    /// <summary>Machine-readable code surfaced in ProblemDetails responses.</summary>
    public string ErrorCode { get; } = errorCode ?? "SPLASHSPHERE_ERROR";
}

// ── HTTP 404 ─────────────────────────────────────────────────────────────────

public class NotFoundException(string message)
    : SplashSphereException(message, "NOT_FOUND");

public sealed class TenantNotFoundException(string tenantId)
    : NotFoundException($"Tenant '{tenantId}' was not found.")
{
    public string TenantId { get; } = tenantId;
}

// ── HTTP 409 / Domain rule violations ────────────────────────────────────────

public class DomainException(string message, string errorCode = "DOMAIN_RULE")
    : SplashSphereException(message, errorCode);

public sealed class InsufficientStockException(string itemName, int requested, int available)
    : DomainException(
        $"Insufficient stock for '{itemName}': requested {requested}, available {available}.",
        "INSUFFICIENT_STOCK")
{
    public string ItemName { get; } = itemName;
    public int Requested { get; } = requested;
    public int Available { get; } = available;
}

public sealed class InvalidTransactionStateException(string currentState, string attemptedAction)
    : DomainException(
        $"Cannot '{attemptedAction}' a transaction that is in '{currentState}' state.",
        "INVALID_TRANSACTION_STATE")
{
    public string CurrentState { get; } = currentState;
    public string AttemptedAction { get; } = attemptedAction;
}

public sealed class PayrollPeriodLockedException(string periodId)
    : DomainException(
        $"Payroll period '{periodId}' is locked and cannot be modified.",
        "PAYROLL_PERIOD_LOCKED")
{
    public string PeriodId { get; } = periodId;
}

// ── HTTP 401 / 403 ───────────────────────────────────────────────────────────

public class UnauthorizedException(string message)
    : SplashSphereException(message, "UNAUTHORIZED");

public class ForbiddenException(string message)
    : SplashSphereException(message, "FORBIDDEN");

// ── HTTP 409 (resource conflict) ─────────────────────────────────────────────

public class ConflictException(string message)
    : SplashSphereException(message, "CONFLICT");

// ── HTTP 422 ─────────────────────────────────────────────────────────────────

public class ValidationException(string message)
    : SplashSphereException(message, "VALIDATION");
