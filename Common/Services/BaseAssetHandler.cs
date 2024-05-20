namespace StardewMods.Common.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Assets;
using StardewMods.Common.Models.Events;

/// <summary>Handles modification and manipulation of assets in the game.</summary>
internal abstract class BaseAssetHandler
{
    private readonly Dictionary<IAssetName, ICachedAsset> cachedAssets = new();
    private readonly IEventManager eventManager;
    private readonly Dictionary<IAssetName, List<ITrackedAsset>> trackedAssets = new();

    /// <summary>Initializes a new instance of the <see cref="BaseAssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    protected BaseAssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModContentHelper modContentHelper)
    {
        this.eventManager = eventManager;
        this.GameContentHelper = gameContentHelper;
        this.ModContentHelper = modContentHelper;

        // Events
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<AssetReadyEventArgs>(this.OnAssetReady);
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the game content helper.</summary>
    protected IGameContentHelper GameContentHelper { get; }

    /// <summary>Gets the mod content helper.</summary>
    protected IModContentHelper ModContentHelper { get; }

    /// <summary>Retrieves the specified asset by name and ensures that it is not null.</summary>
    /// <typeparam name="TAssetType">The type of the asset.</typeparam>
    /// <param name="name">The name of the asset.</param>
    /// <returns>The asset of type TAssetType.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the asset cannot be found or is null.</exception>
    public TAssetType RequireAsset<TAssetType>(string name)
        where TAssetType : notnull
    {
        if (!this.TryGetAsset<TAssetType>(name, out var asset))
        {
            throw new InvalidOperationException($"Failed to get asset: {name}");
        }

        return asset;
    }

    /// <summary>Attempt to get an asset.</summary>
    /// <param name="name">The name of the asset to get.</param>
    /// <param name="asset">When this method returns, contains the asset, if found; otherwise, null.</param>
    /// <typeparam name="TAssetType">The type of asset to return.</typeparam>
    /// <returns><c>true</c> if the asset with the given type is found; otherwise, <c>false</c>.</returns>
    public bool TryGetAsset<TAssetType>(string name, [NotNullWhen(true)] out TAssetType? asset)
        where TAssetType : notnull
    {
        var assetName = this.GameContentHelper.ParseAssetName(name);
        if (!this.cachedAssets.TryGetValue(assetName, out var cachedAsset))
        {
            cachedAsset = new CachedAsset<TAssetType>(() => this.GameContentHelper.Load<TAssetType>(name));
            this.cachedAssets.Add(assetName, cachedAsset);
        }

        if (cachedAsset is not CachedAsset<TAssetType> typedCachedAsset)
        {
            asset = default(TAssetType);
            return false;
        }

        asset = typedCachedAsset.Value;
        return true;
    }

    /// <summary>Adds an asset to load or edit.</summary>
    /// <param name="name">The name of the asset.</param>
    /// <param name="asset">The asset.</param>
    protected void AddAsset(string name, ITrackedAsset asset)
    {
        var assetName = this.GameContentHelper.ParseAssetName(name);
        if (!this.trackedAssets.TryGetValue(assetName, out var assets))
        {
            assets = new List<ITrackedAsset>();
            this.trackedAssets.Add(assetName, assets);
        }

        assets.Add(asset);
    }

    private void OnAssetReady(AssetReadyEventArgs e) { }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (!this.trackedAssets.TryGetValue(e.NameWithoutLocale, out var assets))
        {
            return;
        }

        foreach (var asset in assets)
        {
            asset.ProvideAsset(e);
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        foreach (var name in e.NamesWithoutLocale)
        {
            if (this.cachedAssets.TryGetValue(name, out var cachedAsset))
            {
                cachedAsset.ClearCache();
                this.eventManager.Publish(new AssetInvalidatedEventArgs(name));
            }
        }
    }
}