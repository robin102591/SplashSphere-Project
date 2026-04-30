using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteDisplaySetting;

/// <summary>
/// Removes a per-branch display-setting override. Pass the branch ID; the
/// tenant default (BranchId = null) is permanent and cannot be deleted.
/// </summary>
public sealed record DeleteDisplaySettingCommand(string BranchId) : ICommand;
