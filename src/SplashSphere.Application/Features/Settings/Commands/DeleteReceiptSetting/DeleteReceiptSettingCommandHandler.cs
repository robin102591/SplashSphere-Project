using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteReceiptSetting;

public sealed class DeleteReceiptSettingCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteReceiptSettingCommand, Result>
{
    public async Task<Result> Handle(
        DeleteReceiptSettingCommand request,
        CancellationToken cancellationToken)
    {
        var setting = await context.ReceiptSettings
            .FirstOrDefaultAsync(r => r.BranchId == request.BranchId, cancellationToken);

        // Idempotent — deleting an override that doesn't exist is a no-op.
        // The frontend's "Reset to default" button is only enabled when an
        // override exists, but a stale UI calling this twice should still
        // succeed.
        if (setting is null)
            return Result.Success();

        context.ReceiptSettings.Remove(setting);
        return Result.Success();
    }
}
