namespace StardewMods.Common.Models;

using StardewValley.Extensions;

/// <summary>Represents a table of cached values.</summary>
/// <typeparam name="T">The cached object type.</typeparam>
internal sealed class GenericCacheTable<T> : CacheTable
{
    private readonly Dictionary<string, GenericCachedObject<T>> cachedObjects = [];

    /// <summary>Add or update a value in the collection with the specified key.</summary>
    /// <param name="key">The key of the value to add or update.</param>
    /// <param name="value">The value to add or update.</param>
    public void AddOrUpdate(string key, T value)
    {
        if (this.cachedObjects.TryGetValue(key, out var cachedObject))
        {
            cachedObject.Value = value;
        }
        else
        {
            this.cachedObjects.Add(key, new GenericCachedObject<T>(value));
        }
    }

    /// <inheritdoc />
    public override void RemoveBefore(int ticks) => this.cachedObjects.RemoveWhere(pair => pair.Value.Ticks < ticks);

    /// <summary>Tries to get the data associated with the specified key.</summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key; otherwise, the
    /// default value for the type of the value parameter.
    /// </param>
    /// <returns>true if the key was found; otherwise, false.</returns>
    public bool TryGetValue(string key, out T? value)
    {
        if (this.cachedObjects.TryGetValue(key, out var cachedObject))
        {
            value = cachedObject.Value;
            return true;
        }

        value = default(T);
        return false;
    }
}