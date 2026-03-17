using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueEntry;

public sealed class GetQueueEntryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetQueueEntryQuery, Result<QueueEntryDto>>
{
    public async Task<Result<QueueEntryDto>> Handle(
        GetQueueEntryQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.QueueEntries
            .AsNoTracking()
            .Where(q => q.Id == request.QueueEntryId)
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

        return dto is null
            ? Result.Failure<QueueEntryDto>(Error.NotFound("QueueEntry", request.QueueEntryId))
            : Result.Success(dto);
    }
}
