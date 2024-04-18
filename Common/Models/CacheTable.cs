namespace StardewMods.Common.Models;

using StardewMods.Common.Interfaces;
using StardewValley.Extensions;

/// <inheritdoc cref="StardewMods.Common.Interfaces.ICacheTable" />
internal sealed class CacheTable<T> : CacheTable, ICacheTable<T>
{
    private readonly Dictionary<string, CachedObject<T>> cachedObjects = [];

    public void AddOrUpdate(string key, T value)
    {
        if (this.cachedObjects.TryGetValue(key, out var cachedObject))
        {
            cachedObject.Value = value;
        }
        else
        {
            this.cachedObjects.Add(key, new CachedObject<T>(value));
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override void RemoveBefore(int ticks) => this.cachedObjects.RemoveWhere(pair => pair.Value.Ticks < ticks);
}

/// <inheritdoc cref="StardewMods.Common.Interfaces.ICacheTable" />
internal abstract class CacheTable : ICacheTable
{
    public abstract void RemoveBefore(int ticks);
}