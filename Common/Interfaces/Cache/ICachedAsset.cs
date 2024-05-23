#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Interfaces.Cache;
#else
namespace StardewMods.Common.Interfaces.Cache;
#endif

/// <summary>Represents a cached asset.</summary>
internal interface ICachedAsset
{
    /// <summary>Clear the cached value.</summary>
    void ClearCache();
}