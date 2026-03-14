using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;

public sealed class GetCommissionsReportQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCommissionsReportQuery, CommissionsReportDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<CommissionsReportDto> Handle(
        GetCommissionsReportQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset;
        var toUtc   = request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset;

        // ── Per-employee commission aggregation ───────────────────────────────
        var teQuery = context.TransactionEmployees
            .AsNoTracking()
            .Where(te =>
                te.Transaction.Status == TransactionStatus.Completed &&
                te.Transaction.CompletedAt >= fromUtc &&
                te.Transaction.CompletedAt < toUtc);

        if (request.BranchId is not null)
            teQuery = teQuery.Where(te => te.Employee.BranchId == request.BranchId);

        if (request.EmployeeId is not null)
            teQuery = teQuery.Where(te => te.EmployeeId == request.EmployeeId);

        var rows = await teQuery
            .GroupBy(te => new
            {
                te.EmployeeId,
                te.Employee.FirstName,
                te.Employee.LastName,
                BranchName   = te.Employee.Branch.Name,
                EmployeeType = te.Employee.EmployeeType,
            })
            .Select(g => new EmployeeCommissionDto(
                g.Key.EmployeeId,
                g.Key.FirstName + " " + g.Key.LastName,
                g.Key.BranchName,
                g.Key.EmployeeType.ToString(),
                g.Sum(te => te.TotalCommission),
                g.Count()))
            .OrderByDescending(e => e.TotalCommissions)
            .ToListAsync(cancellationToken);

        var grandTotal       = rows.Sum(r => r.TotalCommissions);
        var transactionCount = rows.Sum(r => r.TransactionCount);

        return new CommissionsReportDto(
            request.From,
            request.To,
            request.BranchId,
            request.EmployeeId,
            grandTotal,
            transactionCount,
            rows);
    }
}
