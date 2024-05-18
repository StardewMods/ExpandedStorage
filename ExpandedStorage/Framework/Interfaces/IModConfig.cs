namespace StardewMods.ExpandedStorage.Framework.Interfaces;

using StardewMods.Common.Models;

/// <summary>Mod config data for Expanded Storage.</summary>
internal interface IModConfig
{
    /// <summary>Gets the default options for different storage types.</summary>
    public Dictionary<string, DefaultStorageOptions> StorageOptions { get; }
}