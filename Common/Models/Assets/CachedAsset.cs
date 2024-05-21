namespace StardewMods.Common.Models.Assets;

using StardewMods.Common.Interfaces.Cache;

/// <inheritdoc />
internal sealed class CachedAsset<TAssetType> : ICachedAsset
    where TAssetType : notnull
{
    private readonly Func<TAssetType> getValue;

    private TAssetType? cachedValue;

    public CachedAsset(Func<TAssetType> getValue) => this.getValue = getValue;

    public TAssetType Value => this.cachedValue ??= this.getValue();

    /// <inheritdoc />
    public void ClearCache() => this.cachedValue = default(TAssetType);
}