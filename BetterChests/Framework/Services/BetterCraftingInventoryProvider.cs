namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterCrafting;
using StardewValley.Inventories;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterCrafting.IInventoryProvider" />
internal sealed class BetterCraftingInventoryProvider : IInventoryProvider
{
    private readonly ContainerHandler containerHandler;

    /// <summary>Initializes a new instance of the <see cref="BetterCraftingInventoryProvider" /> class.</summary>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    public BetterCraftingInventoryProvider(ContainerHandler containerHandler) =>
        this.containerHandler = containerHandler;

    /// <inheritdoc />
    public bool CanExtractItems(object obj, GameLocation? location, Farmer? who) => obj is IStorageContainer;

    /// <inheritdoc />
    public bool CanInsertItems(object obj, GameLocation? location, Farmer? who) => obj is IStorageContainer;

    /// <inheritdoc />
    public void CleanInventory(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.Items.RemoveEmptySlots();

    /// <inheritdoc />
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.Capacity ?? Chest.capacity;

    /// <inheritdoc />
    public IInventory? GetInventory(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.Items;

    /// <inheritdoc />
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who) => (obj as IStorageContainer)?.Items;

    /// <inheritdoc />
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who) => null;

    /// <inheritdoc />
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who) => (obj as IStorageContainer)?.Mutex;

    /// <inheritdoc />
    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who) =>
        (obj as IStorageContainer)?.TileLocation;

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item) =>
        obj is IStorageContainer container && this.containerHandler.CanAddItem(container, item, true);

    /// <inheritdoc />
    public bool IsValid(object obj, GameLocation? location, Farmer? who) => obj is IStorageContainer;
}