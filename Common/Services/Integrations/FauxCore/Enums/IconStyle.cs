#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
#else
namespace StardewMods.Common.Services.Integrations.FauxCore;
#endif

/// <summary>The styles of icon.</summary>
public enum IconStyle
{
    /// <summary>An icon with a transparent background.</summary>
    Transparent,

    /// <summary>An icon with a button background.</summary>
    Button,
}