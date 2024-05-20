namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;

/// <inheritdoc />
internal class GameAsset<TAssetType> : BaseGameAsset
{
    /// <summary>Initializes a new instance of the <see cref="GameAsset{TAssetType}" /> class.</summary>
    /// <param name="entry">The asset entry.</param>
    /// <param name="key">The asset key.</param>
    /// <param name="priority">The edit priority.</param>
    public GameAsset(TAssetType entry, string key, AssetEditPriority priority)
        : base(key, priority) =>
        this.Entry = entry;

    /// <summary>Gets the asset entry.</summary>
    public TAssetType Entry { get; }

    /// <inheritdoc />
    public override void ProvideAsset(AssetRequestedEventArgs e) => e.Edit(this.Apply, this.Priority);

    private void Apply(IAssetData asset)
    {
        var data = asset.AsDictionary<string, TAssetType>().Data;
        data.Add(this.Key, this.Entry);
    }
}