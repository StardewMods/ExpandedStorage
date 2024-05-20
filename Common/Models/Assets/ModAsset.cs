namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;

/// <inheritdoc />
internal class ModAsset<TAssetType> : BaseModAsset
    where TAssetType : notnull
{
    /// <summary>Initializes a new instance of the <see cref="ModAsset{TAssetType}" /> class.</summary>
    /// <param name="path">The asset path.</param>
    /// <param name="priority">The load priority.</param>
    public ModAsset(string path, AssetLoadPriority priority)
        : base(path, priority) { }

    /// <inheritdoc />
    public override void ProvideAsset(AssetRequestedEventArgs e) =>
        e.LoadFromModFile<TAssetType>(this.Path, this.Priority);
}