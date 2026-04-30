using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PosStations.Commands.DeletePosStation;

public sealed record DeletePosStationCommand(string Id) : ICommand;
