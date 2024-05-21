namespace StardewMods.Common.Interfaces.Assets;

using StardewModdingAPI.Events;

/// <summary>Identifies an asset tied to a game path.</summary>
public interface ITrackedAsset
{
    /// <summary>Gets the asset name.</summary>
    IAssetName Name { get; }

    /// <summary>Edits an entry into an asset.</summary>
    /// <param name="key">The entry key.</param>
    /// <param name="getEntry">A function to get the entry.</param>
    /// <param name="priority">The edit priority.</param>
    /// <typeparam name="TEntry">The entry type.</typeparam>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Edit<TEntry>(
        string key,
        Func<TEntry> getEntry,
        AssetEditPriority priority = AssetEditPriority.Default);

    /// <summary>Edits an entry into an asset.</summary>
    /// <param name="key">The entry key.</param>
    /// <param name="editEntry">A method to edit the entry.</param>
    /// <param name="priority">The edit priority.</param>
    /// <typeparam name="TEntry">The entry type.</typeparam>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Edit<TEntry>(
        string key,
        Action<TEntry> editEntry,
        AssetEditPriority priority = AssetEditPriority.Default);

    /// <summary>Edits an asset.</summary>
    /// <param name="apply">The action to apply.</param>
    /// <param name="priority">The edit priority.</param>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Edit(Action<IAssetData> apply, AssetEditPriority priority = AssetEditPriority.Default);

    /// <summary>Invalidate the asset cache.</summary>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Invalidate();

    /// <summary>Provides the asset from a local path.</summary>
    /// <param name="path">The path the load the asset from.</param>
    /// <param name="priority">The load priority.</param>
    /// <typeparam name="TAssetType">The asset type.</typeparam>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Load<TAssetType>(string path, AssetLoadPriority priority = AssetLoadPriority.Exclusive)
        where TAssetType : notnull;

    /// <summary>Provides the asset from a function.</summary>
    /// <param name="load">The function to load the asset.</param>
    /// <param name="priority">The load priority.</param>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Load(Func<object> load, AssetLoadPriority priority = AssetLoadPriority.Exclusive);

    /// <summary>Retrieve the asset or throw an exception if it is null.</summary>
    /// <typeparam name="TAssetType">The asset type.</typeparam>
    /// <returns>An instance of type <typeparamref name="TAssetType" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the asset is null.</exception>
    TAssetType Require<TAssetType>()
        where TAssetType : notnull;

    /// <summary>Tries to get the asset.</summary>
    /// <param name="asset">An instance of type <typeparamref name="TAssetType" />.</param>
    /// <typeparam name="TAssetType">The asset type.</typeparam>
    /// <returns><c>true</c> if the asset could be retrieved; otherwise, <c>false</c>.</returns>
    bool TryGet<TAssetType>([NotNullWhen(true)] out TAssetType? asset)
        where TAssetType : notnull;

    /// <summary>Watch an asset.</summary>
    /// <param name="onInvalidated">The action to perform when the asset is invalidated.</param>
    /// <param name="onReady">The action to perform when the asset is ready.</param>
    /// <param name="onRequested">The action to perform when the asset is requested.</param>
    /// <returns>Returns the tracked asset.</returns>
    ITrackedAsset Watch(
        Action<AssetsInvalidatedEventArgs>? onInvalidated = null,
        Action<AssetReadyEventArgs>? onReady = null,
        Action<AssetRequestedEventArgs>? onRequested = null);
}