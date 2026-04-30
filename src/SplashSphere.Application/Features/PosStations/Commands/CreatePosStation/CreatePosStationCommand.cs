using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PosStations.Commands.CreatePosStation;

/// <summary>
/// Creates a POS station for a branch. Returns the new station ID. Station
/// names are unique within a branch (the same "Counter A" can exist in two
/// different branches).
/// </summary>
public sealed record CreatePosStationCommand(
    string BranchId,
    string Name) : ICommand<string>;
