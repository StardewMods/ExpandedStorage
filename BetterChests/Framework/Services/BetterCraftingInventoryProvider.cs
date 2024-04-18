namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.BetterCrafting;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterCrafting.IInventoryProvider" />
internal sealed class BetterCraftingInventoryProvider : BaseService, IInventoryProvider
{
    private readonly ContainerHandler containerHandler;

    /// <summary>Initializes a new instance of the <see cref="BetterCraftingInventoryProvider" /> class.</summary>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public BetterCraftingInventoryProvider(ContainerHandler containerHandler, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.containerHandler = containerHandler;

    /// <inheritdoc />
    public bool IsValid(object obj, GameLocation? location, Farmer? who) => obj is IStorageContainer;

    /// <inheritdoc />
    public bool CanInsertItems(object obj, GameLocation? location, Farmer? who) => obj is IStorageContainer;

    /// <inheritdoc />
    public bool CanExtractItems(object obj, GameLocation? location, Farmer? who) => obj is IStorageContainer;

    /// <inheritdoc />
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who) => null;

    /// <inheritdoc />
    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.TileLocation;

    /// <inheritdoc />
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who) => (obj as IStorageContainer)?.Mutex;

    /// <inheritdoc />
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who) => (obj as IStorageContainer)?.Items;

    /// <inheritdoc />
    public IInventory? GetInventory(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.Items;

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item) =>
        obj is IStorageContainer container && this.containerHandler.CanAddItem(container, item, true);

    /// <inheritdoc />
    public void CleanInventory(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.Items.RemoveEmptySlots();

    /// <inheritdoc />
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.Capacity ?? Chest.capacity;
}