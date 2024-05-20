namespace StardewMods.Common.Models.Events;

/// <summary>Argument for when a cached asset is invalidated.</summary>
internal sealed class AssetInvalidatedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="AssetInvalidatedEventArgs" /> class.</summary>
    /// <param name="name">The asset name.</param>
    public AssetInvalidatedEventArgs(IAssetName name) => this.Name = name;

    /// <summary>Gets the name of the in validated asset.</summary>
    public IAssetName Name { get; }
}