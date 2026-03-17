using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueDisplay;

public sealed class GetQueueDisplayQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetQueueDisplayQuery, QueueDisplaySnapshotDto>
{
    public async Task<QueueDisplaySnapshotDto> Handle(
        GetQueueDisplayQuery request,
        CancellationToken cancellationToken)
    {
        var activeStatuses = new[] { QueueStatus.Called, QueueStatus.InService, QueueStatus.Waiting };

        var entries = await context.QueueEntries
            .AsNoTracking()
            .IgnoreQueryFilters()   // public endpoint — no tenant filter
            .Where(q => q.BranchId == request.BranchId && activeStatuses.Contains(q.Status))
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .Select(q => new
            {
                q.QueueNumber,
                q.PlateNumber,
                q.Status,
                q.Priority,
                q.EstimatedWaitMinutes,
            })
            .ToListAsync(cancellationToken);

        var calling = entries
            .Where(q => q.Status == QueueStatus.Called)
            .Select(q => new QueueDisplayEntryDto(
                q.QueueNumber,
                MaskPlate(q.PlateNumber),
                q.Status,
                q.Priority,
                q.EstimatedWaitMinutes))
            .ToList();

        var inService = entries
            .Where(q => q.Status == QueueStatus.InService)
            .Select(q => new QueueDisplayEntryDto(
                q.QueueNumber,
                MaskPlate(q.PlateNumber),
                q.Status,
                q.Priority,
                q.EstimatedWaitMinutes))
            .ToList();

        var waitingCount = entries.Count(q => q.Status == QueueStatus.Waiting);

        return new QueueDisplaySnapshotDto(request.BranchId, calling, inService, waitingCount);
    }

    /// <summary>
    /// Masks the middle portion of a plate number for public display.
    /// "ABC 1234" → "ABC***34"
    /// Plates shorter than 5 characters are fully masked as "***".
    /// </summary>
    private static string MaskPlate(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate) || plate.Length < 5)
            return "***";

        var prefix = plate[..3];
        var suffix = plate[^2..];
        return $"{prefix}***{suffix}";
    }
}
