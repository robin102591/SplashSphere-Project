using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    string? EntityType = null,
    string? EntityId = null,
    string? UserId = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<AuditLogDto>>;
