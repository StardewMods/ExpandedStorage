namespace StardewMods.Common.Interfaces;

/// <summary>Represents a table of cached values.</summary>
/// <typeparam name="T">The cached object type.</typeparam>
public interface ICacheTable<T> : ICacheTable
{
    /// <summary>Add or update a value in the collection with the specified key.</summary>
    /// <param name="key">The key of the value to add or update.</param>
    /// <param name="value">The value to add or update.</param>
    public void AddOrUpdate(string key, T value);

    /// <summary>Tries to get the data associated with the specified key.</summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key; otherwise, the
    /// default value for the type of the value parameter.
    /// </param>
    /// <returns>true if the key was found; otherwise, false.</returns>
    public bool TryGetValue(string key, out T? value);
}

/// <summary>Represents a table of cached values.</summary>
public interface ICacheTable { }