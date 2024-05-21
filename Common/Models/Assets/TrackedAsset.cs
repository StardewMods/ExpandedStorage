namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces.Assets;
using StardewMods.Common.Interfaces.Cache;
using StardewMods.Common.Services;

/// <inheritdoc />
internal sealed class TrackedAsset : ITrackedAsset
{
    private readonly BaseAssetHandler assetHandler;
    private readonly List<Action<AssetRequestedEventArgs>> editAsset = [];
    private readonly List<Action<AssetsInvalidatedEventArgs>> watchInvalidated = [];
    private readonly List<Action<AssetReadyEventArgs>> watchReady = [];
    private readonly List<Action<AssetRequestedEventArgs>> watchRequested = [];

    private ICachedAsset? cachedAsset;
    private Action<AssetRequestedEventArgs>? loadAsset;

    public TrackedAsset(BaseAssetHandler assetHandler, IAssetName name)
    {
        this.assetHandler = assetHandler;
        this.Name = name;
    }

    /// <inheritdoc />
    public IAssetName Name { get; }

    /// <inheritdoc />
    public ITrackedAsset Edit<TEntry>(
        string key,
        Func<TEntry> getEntry,
        AssetEditPriority priority = AssetEditPriority.Default)
    {
        this.editAsset.Add(
            e => e.Edit(asset => asset.AsDictionary<string, TEntry>().Data.Add(key, getEntry()), priority));

        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Edit<TEntry>(
        string key,
        Action<TEntry> editEntry,
        AssetEditPriority priority = AssetEditPriority.Default)
    {
        this.editAsset.Add(
            e => e.Edit(
                asset =>
                {
                    if (asset.AsDictionary<string, TEntry>().Data.TryGetValue(key, out var entry))
                    {
                        editEntry(entry);
                    }
                },
                priority));

        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Edit(Action<IAssetData> apply, AssetEditPriority priority = AssetEditPriority.Default)
    {
        this.editAsset.Add(e => e.Edit(apply, priority));
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Invalidate()
    {
        this.assetHandler.GameContentHelper.InvalidateCache(this.Name);
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Load<TAssetType>(string path, AssetLoadPriority priority = AssetLoadPriority.Exclusive)
        where TAssetType : notnull
    {
        this.loadAsset ??= e => e.LoadFromModFile<TAssetType>(path, priority);
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Load(Func<object> load, AssetLoadPriority priority = AssetLoadPriority.Exclusive)
    {
        this.loadAsset ??= e => e.LoadFrom(load, priority);
        return this;
    }

    /// <summary>Asset ready event.</summary>
    /// <param name="e">Event arguments.</param>
    public void OnAssetReady(AssetReadyEventArgs e)
    {
        foreach (var action in this.watchReady)
        {
            action(e);
        }
    }

    /// <summary>Asset requested event.</summary>
    /// <param name="e">Event arguments.</param>
    public void OnAssetRequested(AssetRequestedEventArgs e)
    {
        this.loadAsset?.Invoke(e);
        foreach (var edit in this.editAsset)
        {
            edit(e);
        }

        foreach (var action in this.watchRequested)
        {
            action(e);
        }
    }

    /// <summary>Assets invalidated event.</summary>
    /// <param name="e">Event arguments.</param>
    public void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        this.cachedAsset?.ClearCache();
        foreach (var action in this.watchInvalidated)
        {
            action(e);
        }
    }

    /// <inheritdoc />
    public TAssetType Require<TAssetType>()
        where TAssetType : notnull
    {
        if (!this.TryGet(out TAssetType? asset))
        {
            throw new InvalidOperationException($"Failed to get asset: {this.Name}");
        }

        return asset;
    }

    /// <inheritdoc />
    public bool TryGet<TAssetType>([NotNullWhen(true)] out TAssetType? asset)
        where TAssetType : notnull
    {
        this.cachedAsset ??=
            new CachedAsset<TAssetType>(() => this.assetHandler.GameContentHelper.Load<TAssetType>(this.Name));

        if (this.cachedAsset is CachedAsset<TAssetType> cachedAssetWithType)
        {
            asset = cachedAssetWithType.Value;
            return true;
        }

        asset = default(TAssetType);
        return false;
    }

    /// <inheritdoc />
    public ITrackedAsset Watch(
        Action<AssetsInvalidatedEventArgs>? onInvalidated = null,
        Action<AssetReadyEventArgs>? onReady = null,
        Action<AssetRequestedEventArgs>? onRequested = null)
    {
        if (onInvalidated != null)
        {
            this.watchInvalidated.Add(onInvalidated);
        }

        if (onReady != null)
        {
            this.watchReady.Add(onReady);
        }

        if (onRequested != null)
        {
            this.watchRequested.Add(onRequested);
        }

        return this;
    }
}