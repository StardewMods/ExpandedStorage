#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Interfaces;
#else

namespace StardewMods.Common.Interfaces;
#endif

/// <summary>Provides an integration point for using external mods' APIs.</summary>
public interface IModIntegration
{
    /// <summary>Gets a value indicating whether the mod is loaded.</summary>
    protected internal bool IsLoaded { get; }

    /// <summary>Gets metadata for this mod.</summary>
    protected internal IModInfo? ModInfo { get; }

    /// <summary>Gets the Unique Id for this mod.</summary>
    protected internal string UniqueId { get; }

    /// <summary>Gets the minimum supported version for this mod.</summary>
    protected internal ISemanticVersion? Version { get; }
}