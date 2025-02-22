#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Models.Cache;
#else

namespace StardewMods.Common.Models.Cache;
#endif

/// <summary>Represents a table of cached values.</summary>
public abstract class BaseCacheTable
{
    /// <summary>Removes all cached values that have not been accessed since before the specified tick count.</summary>
    /// <param name="ticks">The number of ticks.</param>
    public abstract void RemoveBefore(int ticks);
}