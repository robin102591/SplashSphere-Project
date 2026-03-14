namespace SplashSphere.SharedKernel.Abstractions;

/// <summary>
/// Marks an entity as having auditable timestamps managed by <c>AuditableEntityInterceptor</c>.
/// Both properties are stored as UTC and should be converted to Asia/Manila on display.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
