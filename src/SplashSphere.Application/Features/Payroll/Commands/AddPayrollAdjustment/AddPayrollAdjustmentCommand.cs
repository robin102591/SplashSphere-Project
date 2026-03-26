using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll.Commands.AddPayrollAdjustment;

public sealed record AddPayrollAdjustmentCommand(
    string EntryId,
    AdjustmentType Type,
    string Category,
    decimal Amount,
    string? Notes,
    string? TemplateId) : ICommand<string>;
