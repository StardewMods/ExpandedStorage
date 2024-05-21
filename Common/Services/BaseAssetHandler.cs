namespace StardewMods.Common.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Interfaces.Assets;
using StardewMods.Common.Models.Assets;
using StardewMods.Common.Services.Integrations.ContentPatcher;

/// <inheritdoc />
internal abstract class BaseAssetHandler : IAssetHandler
{
    private readonly Dictionary<Func<AssetRequestedEventArgs, bool>, Action<ITrackedAsset>> dynamicAssets = [];
    private readonly Dictionary<IAssetName, TrackedAsset> trackedAssets = new();

    private bool initialized;

    /// <summary>Initializes a new instance of the <see cref="BaseAssetHandler" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    protected BaseAssetHandler(
        ContentPatcherIntegration contentPatcherIntegration,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModContentHelper modContentHelper)
    {
        this.GameContentHelper = gameContentHelper;
        this.ModContentHelper = modContentHelper;

        // Events
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<AssetReadyEventArgs>(this.OnAssetReady);
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);

        if (contentPatcherIntegration.IsLoaded)
        {
            eventManager.Subscribe<ConditionsApiReadyEventArgs>(_ => this.OnInitialLoad());
            return;
        }

        eventManager.Subscribe<GameLaunchedEventArgs>(_ => this.OnInitialLoad());
    }

    /// <summary>Gets the game content helper.</summary>
    public IGameContentHelper GameContentHelper { get; }

    /// <summary>Gets the mod content helper.</summary>
    public IModContentHelper ModContentHelper { get; }

    /// <inheritdoc />
    public ITrackedAsset Asset(string name)
    {
        var assetName = this.GameContentHelper.ParseAssetName(name);
        return this.Asset(assetName);
    }

    /// <inheritdoc />
    public ITrackedAsset Asset(IAssetName assetName)
    {
        if (this.trackedAssets.TryGetValue(assetName, out var trackedAsset))
        {
            return trackedAsset;
        }

        trackedAsset = new TrackedAsset(this, assetName);
        this.trackedAssets.Add(assetName, trackedAsset);
        return trackedAsset;
    }

    /// <inheritdoc />
    public void DynamicAsset(Func<AssetRequestedEventArgs, bool> predicate, Action<ITrackedAsset> action) =>
        this.dynamicAssets.Add(predicate, action);

    private void OnAssetReady(AssetReadyEventArgs e)
    {
        var actionQueue = new Queue<Action<AssetReadyEventArgs>>();
        foreach (var (assetName, trackedAsset) in this.trackedAssets)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(assetName))
            {
                actionQueue.Enqueue(trackedAsset.OnAssetReady);
            }
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action(e);
        }
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        var actionQueue = new Queue<Action<AssetRequestedEventArgs>>();
        foreach (var (assetName, trackedAsset) in this.trackedAssets)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(assetName))
            {
                actionQueue.Enqueue(trackedAsset.OnAssetRequested);
            }
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action(e);
        }

        foreach (var (predicate, action) in this.dynamicAssets)
        {
            if (predicate(e))
            {
                action(this.Asset(e.NameWithoutLocale));
            }
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        var actionQueue = new Queue<Action<AssetsInvalidatedEventArgs>>();
        foreach (var (assetName, trackedAsset) in this.trackedAssets)
        {
            if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo(assetName)))
            {
                actionQueue.Enqueue(trackedAsset.OnAssetsInvalidated);
            }
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action(e);
        }
    }

    private void OnInitialLoad()
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        var actionQueue = new Queue<Action>();
        foreach (var (_, trackedAsset) in this.trackedAssets)
        {
            actionQueue.Enqueue(trackedAsset.OnInitialLoad);
        }

        while (actionQueue.TryDequeue(out var action))
        {
            action();
        }
    }
}