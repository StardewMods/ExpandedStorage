namespace StardewMods.ExpandedStorage.Framework.Models;

using StardewMods.Common.Models;
using StardewMods.ExpandedStorage.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions { get; set; } = [];
}