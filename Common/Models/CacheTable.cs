namespace StardewMods.Common.Models;

/// <summary>Represents a table of cached values.</summary>
internal abstract class CacheTable
{
    /// <summary>Removes all cached values that have not been accessed since before the specified tick count.</summary>
    /// <param name="ticks">The number of ticks.</param>
    public abstract void RemoveBefore(int ticks);
}