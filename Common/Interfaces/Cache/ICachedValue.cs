#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Interfaces.Cache;
#else
namespace StardewMods.Common.Interfaces.Cache;
#endif

/// <summary>Represents a cached value.</summary>
internal interface ICachedValue
{
    /// <summary>Gets the original value.</summary>
    string OriginalValue { get; }
}