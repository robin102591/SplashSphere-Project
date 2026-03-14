using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueue;

public sealed class GetQueueQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetQueueQuery, PagedResult<QueueEntryDto>>
{
    private static readonly QueueStatus[] DefaultStatuses =
        [QueueStatus.Waiting, QueueStatus.Called, QueueStatus.InService];

    public async Task<PagedResult<QueueEntryDto>> Handle(
        GetQueueQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = request.Statuses is { Count: > 0 }
            ? request.Statuses
            : DefaultStatuses;

        var query = context.QueueEntries
            .AsNoTracking()
            .Where(q => q.BranchId == request.BranchId && statuses.Contains(q.Status));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(q => new QueueEntryDto(
                q.Id,
                q.BranchId,
                q.Branch.Name,
                q.QueueNumber,
                q.PlateNumber,
                q.Status,
                q.Priority,
                q.CustomerId,
                q.Customer != null ? q.Customer.FirstName + " " + q.Customer.LastName : null,
                q.CarId,
                q.TransactionId,
                q.EstimatedWaitMinutes,
                q.PreferredServices,
                q.Notes,
                q.CalledAt,
                q.StartedAt,
                q.CompletedAt,
                q.CancelledAt,
                q.NoShowAt,
                q.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<QueueEntryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
