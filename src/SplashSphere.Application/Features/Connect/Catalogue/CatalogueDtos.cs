namespace SplashSphere.Application.Features.Connect.Catalogue;

/// <summary>Global vehicle make for the Connect app's vehicle picker.</summary>
public sealed record GlobalMakeDto(string Id, string Name, int DisplayOrder);

/// <summary>Global vehicle model, scoped to a make.</summary>
public sealed record GlobalModelDto(string Id, string MakeId, string Name, int DisplayOrder);
