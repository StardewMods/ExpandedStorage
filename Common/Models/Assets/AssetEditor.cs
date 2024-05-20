namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;

/// <summary>Represents an asset to be edited.</summary>
internal sealed class AssetEditor : ITrackedAsset
{
    private readonly Action<IAssetData> assetEditor;
    private readonly AssetEditPriority priority;

    /// <summary>Initializes a new instance of the <see cref="AssetEditor" /> class.</summary>
    /// <param name="assetEditor">The action to apply to the asset.</param>
    /// <param name="priority">The edit priority.</param>
    public AssetEditor(Action<IAssetData> assetEditor, AssetEditPriority priority)
    {
        this.assetEditor = assetEditor;
        this.priority = priority;
    }

    /// <inheritdoc />
    public void ProvideAsset(AssetRequestedEventArgs e) => e.Edit(this.assetEditor, this.priority);
}