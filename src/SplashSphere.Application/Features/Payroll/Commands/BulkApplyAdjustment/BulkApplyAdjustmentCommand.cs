using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll.Commands.BulkApplyAdjustment;

public sealed record BulkApplyAdjustmentCommand(
    IReadOnlyList<string> EntryIds,
    AdjustmentType AdjustmentType,
    decimal Amount,
    string? Notes,
    string? TemplateId = null) : ICommand;
