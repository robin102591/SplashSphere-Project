using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Queue.Queries.GetNextInQueue;

public sealed class GetNextInQueueQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNextInQueueQuery, QueueEntryDto?>
{
    public async Task<QueueEntryDto?> Handle(
        GetNextInQueueQuery request,
        CancellationToken cancellationToken)
    {
        return await context.QueueEntries
            .AsNoTracking()
            .Where(q => q.BranchId == request.BranchId && q.Status == QueueStatus.Waiting)
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
