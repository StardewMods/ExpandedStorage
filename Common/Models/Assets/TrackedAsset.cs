#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Assets;

using StardewModdingAPI.Events;
using StardewMods.FauxCore.Common.Interfaces.Assets;
using StardewMods.FauxCore.Common.Interfaces.Cache;
using StardewMods.FauxCore.Common.Services;

#else
namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces.Assets;
using StardewMods.Common.Interfaces.Cache;
using StardewMods.Common.Services;
#endif

/// <inheritdoc />
internal sealed class TrackedAsset : ITrackedAsset
{
    private readonly BaseAssetHandler assetHandler;
    private readonly Queue<Action> deferred = [];
    private readonly List<Action<AssetRequestedEventArgs>> editAsset = [];
    private readonly List<Action> watchInitialLoad = [];
    private readonly List<Action<AssetsInvalidatedEventArgs>> watchInvalidated = [];
    private readonly List<Action<AssetReadyEventArgs>> watchReady = [];
    private readonly List<Action<AssetRequestedEventArgs>> watchRequested = [];

    private ICachedAsset? cachedAsset;
    private bool initialized;
    private Action<AssetRequestedEventArgs>? loadAsset;

    /// <summary>Initializes a new instance of the <see cref="TrackedAsset" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="name">The asset name.</param>
    public TrackedAsset(BaseAssetHandler assetHandler, IAssetName name)
    {
        this.assetHandler = assetHandler;
        this.Name = name;
    }

    /// <inheritdoc />
    public IAssetName Name { get; }

    /// <inheritdoc />
    public bool Invalidated { get; private set; }

    /// <inheritdoc />
    public ITrackedAsset Edit<TEntry>(
        string key,
        Func<TEntry> getEntry,
        AssetEditPriority priority = AssetEditPriority.Default)
    {
        this.editAsset.Add(
            e => e.Edit(asset => asset.AsDictionary<string, TEntry>().Data.Add(key, getEntry()), priority));

        this.Defer(() => Log.Trace("Asset: {0}, Entry: {1}, Mode: Add, Priority: {2}", this.Name, key, priority));
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

        this.Defer(() => Log.Trace("Asset: {0}, Entry: {1}, Mode: Edit, Priority: {2}", this.Name, key, priority));
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Edit(Action<IAssetData> apply, AssetEditPriority priority = AssetEditPriority.Default)
    {
        this.editAsset.Add(e => e.Edit(apply, priority));
        this.Defer(() => Log.Trace("Asset: {0}, Mode: Apply, Priority: {1}", this.Name, priority));
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Invalidate()
    {
        if (this.Invalidated)
        {
            return this;
        }

        this.Invalidated = true;
        this.assetHandler.GameContentHelper.InvalidateCache(this.Name);
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Load<TAssetType>(string path, AssetLoadPriority priority = AssetLoadPriority.Exclusive)
        where TAssetType : notnull
    {
        this.loadAsset ??= e => e.LoadFromModFile<TAssetType>(path, priority);
        this.Defer(() => Log.Trace("Asset: {0}, Mode: Load, Source: {1}, Priority: {2}", this.Name, path, priority));
        return this;
    }

    /// <inheritdoc />
    public ITrackedAsset Load(Func<object> load, AssetLoadPriority priority = AssetLoadPriority.Exclusive)
    {
        this.loadAsset ??= e => e.LoadFrom(load, priority);
        this.Defer(() => Log.Trace("Asset: {0}, Mode: Load, Source: Func, Priority: {1}", this.Name, priority));
        return this;
    }

    /// <summary>Asset ready event.</summary>
    /// <param name="e">Event arguments.</param>
    public void OnAssetReady(AssetReadyEventArgs e)
    {
        this.Invalidated = false;
        var actionQueue = new Queue<Action<AssetReadyEventArgs>>();
        foreach (var action in this.watchReady)
        {
            actionQueue.Enqueue(action);
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action(e);
        }
    }

    /// <summary>Asset requested event.</summary>
    /// <param name="e">Event arguments.</param>
    public void OnAssetRequested(AssetRequestedEventArgs e)
    {
        this.Invalidated = false;
        var actionQueue = new Queue<Action<AssetRequestedEventArgs>>();
        if (this.loadAsset is not null)
        {
            actionQueue.Enqueue(this.loadAsset);
        }

        foreach (var edit in this.editAsset)
        {
            actionQueue.Enqueue(edit);
        }

        foreach (var action in this.watchRequested)
        {
            actionQueue.Enqueue(action);
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action(e);
        }
    }

    /// <summary>Assets invalidated event.</summary>
    /// <param name="e">Event arguments.</param>
    public void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        this.cachedAsset?.ClearCache();
        var actionQueue = new Queue<Action<AssetsInvalidatedEventArgs>>();
        foreach (var action in this.watchInvalidated)
        {
            actionQueue.Enqueue(action);
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action(e);
        }
    }

    /// <summary>Initial load event.</summary>
    public void OnInitialLoad()
    {
        this.initialized = true;
        var actionQueue = new Queue<Action>();
        while (this.deferred.TryDequeue(out var action))
        {
            actionQueue.Enqueue(action);
        }

        foreach (var action in this.watchInitialLoad)
        {
            actionQueue.Enqueue(action);
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action();
        }

        this.Invalidate();
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
        Action? onInitialLoad = null,
        Action<AssetsInvalidatedEventArgs>? onInvalidated = null,
        Action<AssetReadyEventArgs>? onReady = null,
        Action<AssetRequestedEventArgs>? onRequested = null)
    {
        if (onInitialLoad != null)
        {
            this.watchInitialLoad.Add(onInitialLoad);
        }

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

    private void Defer(Action action)
    {
        if (this.initialized)
        {
            action();
            return;
        }

        this.deferred.Enqueue(action);
    }
}