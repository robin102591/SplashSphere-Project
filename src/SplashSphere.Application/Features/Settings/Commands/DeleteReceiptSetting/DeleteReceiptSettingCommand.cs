using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteReceiptSetting;

/// <summary>
/// Removes the per-branch receipt-setting override identified by
/// <see cref="BranchId"/>. After deletion, the branch falls back to the
/// tenant default. The tenant default itself (BranchId = null) cannot be
/// deleted — there is always a default to fall through to.
/// </summary>
public sealed record DeleteReceiptSettingCommand(string BranchId) : ICommand;
