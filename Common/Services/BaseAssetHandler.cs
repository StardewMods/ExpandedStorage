namespace StardewMods.Common.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Interfaces.Assets;
using StardewMods.Common.Models.Assets;

/// <inheritdoc />
internal abstract class BaseAssetHandler : IAssetHandler
{
    private readonly Dictionary<Func<AssetRequestedEventArgs, bool>, Action<ITrackedAsset>> dynamicAssets = [];
    private readonly Dictionary<IAssetName, TrackedAsset> trackedAssets = new();

    /// <summary>Initializes a new instance of the <see cref="BaseAssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    protected BaseAssetHandler(
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
        foreach (var (assetName, trackedAsset) in this.trackedAssets)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(assetName))
            {
                trackedAsset.OnAssetReady(e);
            }
        }
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        foreach (var (assetName, trackedAsset) in this.trackedAssets)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(assetName))
            {
                trackedAsset.OnAssetRequested(e);
            }
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
        foreach (var (assetName, trackedAsset) in this.trackedAssets)
        {
            if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo(assetName)))
            {
                trackedAsset.OnAssetsInvalidated(e);
            }
        }
    }
}