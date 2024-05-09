namespace StardewMods.Common.Models.Cache;

/// <summary>Represents a cached object.</summary>
internal class BaseCachedObject
{
    /// <summary>Initializes a new instance of the <see cref="BaseCachedObject" /> class.</summary>
    protected BaseCachedObject() => this.Ticks = Game1.ticks;

    /// <summary>Gets the number of ticks since the cached object was last accessed.</summary>
    public int Ticks { get; protected set; }
}