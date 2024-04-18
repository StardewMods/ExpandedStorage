namespace StardewMods.Common.Interfaces;

/// <summary>Represents a cached object.</summary>
/// <typeparam name="T">The cached object type.</typeparam>
public interface ICachedObject<out T>
{
    /// <summary>Gets the value of the cached object.</summary>
    T Value { get; }
}

/// <summary>Represents a cached object.</summary>
public interface ICachedObject { }