using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteDisplaySetting;

public sealed class DeleteDisplaySettingCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDisplaySettingCommand, Result>
{
    public async Task<Result> Handle(
        DeleteDisplaySettingCommand request,
        CancellationToken cancellationToken)
    {
        var setting = await context.DisplaySettings
            .FirstOrDefaultAsync(d => d.BranchId == request.BranchId, cancellationToken);

        // Idempotent — deleting an override that doesn't exist is a no-op.
        if (setting is null)
            return Result.Success();

        context.DisplaySettings.Remove(setting);
        return Result.Success();
    }
}
