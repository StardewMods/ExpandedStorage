#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Interfaces.Assets;

using StardewModdingAPI.Events;

#else
namespace StardewMods.Common.Interfaces.Assets;

using StardewModdingAPI.Events;
#endif

/// <summary>Handles modification and manipulation of assets in the game.</summary>
public interface IAssetHandler
{
    /// <summary>Gets a tracked asset.</summary>
    /// <param name="name">The asset name.</param>
    /// <returns>Returns the tracked asset.</returns>
    public ITrackedAsset Asset(string name);

    /// <summary>Gets a tracked asset.</summary>
    /// <param name="assetName">The asset name.</param>
    /// <returns>Returns the tracked asset.</returns>
    public ITrackedAsset Asset(IAssetName assetName);

    /// <summary>Add an action to a tracked asset based on a predicate.</summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="action">The action.</param>
    public void DynamicAsset(Func<AssetRequestedEventArgs, bool> predicate, Action<ITrackedAsset> action);
}