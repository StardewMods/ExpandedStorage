namespace StardewMods.Common.Models;

/// <summary>Represents a cached object.</summary>
/// <typeparam name="T">The cached object type.</typeparam>
internal sealed class GenericCachedObject<T> : CachedObject
{
    private T value;

    /// <summary>Initializes a new instance of the <see cref="GenericCachedObject{T}" /> class.</summary>
    /// <param name="value">The initial value.</param>
    public GenericCachedObject(T value) => this.value = value;

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