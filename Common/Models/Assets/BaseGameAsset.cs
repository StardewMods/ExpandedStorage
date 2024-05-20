namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;

/// <summary>Represents an asset to be edited.</summary>
internal abstract class BaseGameAsset : ITrackedAsset
{
    /// <summary>Initializes a new instance of the <see cref="BaseGameAsset" /> class.</summary>
    /// <param name="key">The asset key.</param>
    /// <param name="priority">The edit priority.</param>
    protected BaseGameAsset(string key, AssetEditPriority priority)
    {
        this.Key = key;
        this.Priority = priority;
    }

    /// <summary>Gets the key for the asset entry.</summary>
    public string Key { get; }

    /// <summary>Gets the priority for editing the asset.</summary>
    public AssetEditPriority Priority { get; }

    /// <inheritdoc />
    public abstract void ProvideAsset(AssetRequestedEventArgs e);
}