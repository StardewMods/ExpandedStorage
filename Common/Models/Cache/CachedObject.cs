namespace StardewMods.Common.Models.Cache;

/// <summary>Represents a cached object.</summary>
/// <typeparam name="T">The cached object type.</typeparam>
internal sealed class CachedObject<T> : BaseCachedObject
{
    private T value;

    /// <summary>Initializes a new instance of the <see cref="CachedObject{T}" /> class.</summary>
    /// <param name="value">The initial value.</param>
    public CachedObject(T value) => this.value = value;

    /// <summary>Gets or sets the value of the cached object.</summary>
    public T Value
    {
        get
        {
            this.Ticks = Game1.ticks;
            return this.value;
        }

        set
        {
            this.Ticks = Game1.ticks;
            this.value = value;
        }
    }
}