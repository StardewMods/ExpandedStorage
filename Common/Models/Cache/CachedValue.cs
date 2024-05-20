namespace StardewMods.Common.Models.Cache;

using StardewMods.Common.Interfaces;

/// <inheritdoc />
public readonly struct CachedValue<T> : ICachedValue
{
    /// <summary>Initializes a new instance of the <see cref="CachedValue{T}" /> struct.</summary>
    /// <param name="originalValue">The original value.</param>
    /// <param name="cachedValue">The cached value.</param>
    public CachedValue(string originalValue, T cachedValue)
    {
        this.OriginalValue = originalValue;
        this.Value = cachedValue;
    }

    /// <inheritdoc />
    public string OriginalValue { get; }

    /// <summary>Gets the cached value.</summary>
    public T Value { get; }
}