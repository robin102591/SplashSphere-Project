using MediatR;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayslip;

public sealed record GetPayslipQuery(string EntryId) : IRequest<PayslipDto?>;
