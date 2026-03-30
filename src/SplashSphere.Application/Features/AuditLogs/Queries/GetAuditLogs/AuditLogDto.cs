namespace SplashSphere.Application.Features.AuditLogs.Queries.GetAuditLogs;

public sealed record AuditLogDto(
    string Id,
    string? UserId,
    string Action,
    string EntityType,
    string EntityId,
    string? Changes,
    DateTime Timestamp);
