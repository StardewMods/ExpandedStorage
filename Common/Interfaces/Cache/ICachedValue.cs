namespace StardewMods.Common.Interfaces.Cache;

/// <summary>Represents a cached value.</summary>
internal interface ICachedValue
{
    /// <summary>Gets the original value.</summary>
    string OriginalValue { get; }
}