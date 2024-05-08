namespace StardewMods.Common.Models;

/// <summary>Represents a cached object.</summary>
internal class CachedObject
{
    /// <summary>Initializes a new instance of the <see cref="CachedObject" /> class.</summary>
    protected CachedObject() => this.Ticks = Game1.ticks;

    /// <summary>Gets the number of ticks since the cached object was last accessed.</summary>
    public int Ticks { get; protected set; }
}