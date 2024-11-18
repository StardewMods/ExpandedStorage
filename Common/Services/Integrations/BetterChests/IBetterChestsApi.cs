#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.BetterChests;
#else

namespace StardewMods.Common.Services.Integrations.BetterChests;
#endif

using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Mod API for Better Chests.</summary>
public interface IBetterChestsApi
{
    /// <summary>Adds config options for the storage type.</summary>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="pageId">The page id if a new page should be added, or null.</param>
    /// <param name="getTitle">A function to return the page title, or null.</param>
    /// <param name="options">The options to configure.</param>
    public void AddConfigOptions(IManifest manifest, string? pageId, Func<string>? getTitle, IStorageOptions options);

    /// <summary>Sort a a container.</summary>
    /// <param name="container">The container to sort.</param>
    /// <param name="reverse">Indicates whether to reverse the sort order.</param>
    public void Sort(IStorageContainer container, bool reverse = false);

    /// <summary>Transfers items from one container to another.</summary>
    /// <param name="containerFrom">The container to transfer items from.</param>
    /// <param name="containerTo">The container to transfer items to.</param>
    /// <returns>Returns the items that were transferred.</returns>
    public IEnumerable<(string ItemId, int Amount, bool Prevented)> Transfer(
        IStorageContainer containerFrom,
        IStorageContainer containerTo);

    /// <summary>Tries to retrieve a container from the specified game location at the specified position.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="pos">The position of the game location where the container will be retrieved.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to retrieve a container from the active menu.</summary>
    /// <param name="menu">The menu to retrieve a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(IClickableMenu menu, [NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The player whose container will be retrieved.</param>
    /// <param name="index">The index of the player's inventory. Defaults to the active item.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Farmer farmer, int index, [NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to retrieve a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IStorageContainer? container);
}