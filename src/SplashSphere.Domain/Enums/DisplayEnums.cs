namespace SplashSphere.Domain.Enums;

// ── Customer display appearance ──────────────────────────────────────────────

/// <summary>Visual theme of the customer-facing display.</summary>
public enum DisplayTheme
{
    Dark  = 0,  // Navy background, white text — high-contrast counter display
    Light = 1,  // White background, dark text — bright environments
    Brand = 2,  // Uses the tenant's primary brand color as accent
}

/// <summary>Body font size on the customer display.</summary>
public enum DisplayFontSize
{
    Normal     = 0,
    Large      = 1,
    ExtraLarge = 2,
}

/// <summary>Display orientation. Most counter-mounted screens are landscape.</summary>
public enum DisplayOrientation
{
    Landscape = 0,
    Portrait  = 1,
}
