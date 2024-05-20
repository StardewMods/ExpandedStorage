namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;

/// <inheritdoc />
internal class ModAsset<TAssetType> : BaseModAsset
    where TAssetType : notnull
{
    private readonly Action<AssetRequestedEventArgs> provideAsset;

    /// <summary>Initializes a new instance of the <see cref="ModAsset{TAssetType}" /> class.</summary>
    /// <param name="path">The asset path.</param>
    /// <param name="priority">The load priority.</param>
    public ModAsset(string path, AssetLoadPriority priority)
        : base(priority) =>
        this.provideAsset = e => e.LoadFromModFile<TAssetType>(path, priority);

    public ModAsset(Func<object> loadAsset, AssetLoadPriority priority)
        : base(priority) =>
        this.provideAsset = e => e.LoadFrom(loadAsset, priority);

    /// <inheritdoc />
    public override void ProvideAsset(AssetRequestedEventArgs e) => this.provideAsset(e);
}