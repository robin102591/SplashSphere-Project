namespace SplashSphere.Infrastructure.Authentication;

/// <summary>
/// Marks an endpoint as requiring a specific plan feature.
/// The <see cref="PlanEnforcementMiddleware"/> reads this attribute and returns
/// 403 if the tenant's plan does not include the feature.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresFeatureAttribute(string featureKey) : Attribute
{
    public string FeatureKey { get; } = featureKey;
}
