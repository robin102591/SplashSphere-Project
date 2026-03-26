using MediatR;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollEntryDetail;

public sealed record GetPayrollEntryDetailQuery(string EntryId) : IRequest<PayrollEntryDetailDto?>;
