namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.Buildings;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class BuildingContainer : BaseContainer<Building>
{
    private readonly Chest? chest;

    /// <summary>Initializes a new instance of the <see cref="BuildingContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="building">The building to which the storage is connected.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public BuildingContainer(IStorageOptions baseOptions, Building building, Chest chest)
        : base(baseOptions)
    {
        this.Source = new WeakReference<Building>(building);
        this.chest = chest;
    }

    /// <summary>Initializes a new instance of the <see cref="BuildingContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="shippingBin">The building to which the storage is connected.</param>
    public BuildingContainer(IStorageOptions baseOptions, ShippingBin shippingBin)
        : base(baseOptions) =>
        this.Source = new WeakReference<Building>(shippingBin);

    /// <summary>Gets the source building of the container.</summary>
    public Building Building =>
        this.Source.TryGetTarget(out var target)
            ? target
            : throw new ObjectDisposedException(nameof(BuildingContainer));

    /// <inheritdoc />
    public override int Capacity => this.chest?.GetActualCapacity() ?? int.MaxValue;

    /// <inheritdoc />
    public override IInventory Items => this.chest?.GetItemsForPlayer() ?? Game1.getFarm().getShippingBin(Game1.player);

    /// <inheritdoc />
    public override GameLocation Location => this.Building.GetParentLocation();

    /// <inheritdoc />
    public override Vector2 TileLocation =>
        new(
            this.Building.tileX.Value + (this.Building.tilesWide.Value / 2f),
            this.Building.tileY.Value + (this.Building.tilesHigh.Value / 2f));

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Building.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.chest?.GetMutex();

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<Building> Source { get; }

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false)
    {
        switch (this.Building)
        {
            case ShippingBin shippingBin:
                if (playSound)
                {
                    Game1.player.currentLocation.localSound("shwip");
                }

                ItemGrabMenu itemGrabMenu;

                if (this.Options.ResizeChest is ChestMenuOption.Default or ChestMenuOption.Disabled)
                {
                    itemGrabMenu = new ItemGrabMenu(
                        null,
                        true,
                        false,
                        Utility.highlightShippableObjects,
                        this.GrabItemFromInventory,
                        null,
                        null,
                        true,
                        true,
                        false,
                        true,
                        false,
                        0,
                        null,
                        -1,
                        shippingBin);

                    itemGrabMenu.initializeUpperRightCloseButton();
                    itemGrabMenu.setBackgroundTransparency(b: false);
                    itemGrabMenu.setDestroyItemOnClick(b: true);
                    itemGrabMenu.initializeShippingBin();
                }
                else
                {
                    itemGrabMenu = new ItemGrabMenu(
                        this.Items,
                        false,
                        true,
                        Utility.highlightShippableObjects,
                        this.GrabItemFromInventory,
                        null,
                        this.GrabItemFromChest,
                        false,
                        true,
                        true,
                        true,
                        true,
                        0,
                        null,
                        -1,
                        shippingBin);
                }

                Game1.activeClickableMenu = itemGrabMenu;

                break;

            case JunimoHut when this.chest is not null:
                Game1.activeClickableMenu = new ItemGrabMenu(
                    this.Items,
                    false,
                    true,
                    InventoryMenu.highlightAllItems,
                    this.GrabItemFromInventory,
                    null,
                    this.GrabItemFromChest,
                    false,
                    true,
                    true,
                    true,
                    true,
                    1,
                    null,
                    1,
                    this.Building);

                break;

            default:
                if (this.chest is not null)
                {
                    if (playSound)
                    {
                        Game1.player.currentLocation.localSound("openChest");
                    }

                    Game1.activeClickableMenu = new ItemGrabMenu(
                        this.Items,
                        false,
                        true,
                        InventoryMenu.highlightAllItems,
                        this.GrabItemFromInventory,
                        null,
                        this.GrabItemFromChest,
                        false,
                        true,
                        true,
                        true,
                        true,
                        1,
                        null,
                        -1,
                        this.Building);
                }

                break;
        }
    }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        if (this.chest is not null)
        {
            remaining = this.chest.addItem(item);
            return remaining is null || remaining.Stack != stack;
        }

        remaining = null;
        foreach (var slot in this.Items)
        {
            if (slot?.canStackWith(item) != true)
            {
                continue;
            }

            item.Stack = slot.addToStack(item);
            if (item.Stack > 0)
            {
                continue;
            }

            Game1.getFarm().lastItemShipped = slot;
            return true;
        }

        this.Items.Add(item);
        Game1.getFarm().lastItemShipped = item;
        return true;
    }

    /// <inheritdoc />
    public override bool TryRemove(Item item)
    {
        if (!this.Items.Contains(item))
        {
            return false;
        }

        this.Items.Remove(item);
        this.Items.RemoveEmptySlots();
        return true;
    }

    /// <inheritdoc />
    public override void GrabItemFromChest(Item item, Farmer who)
    {
        if (item is null || !who.couldInventoryAcceptThisItem(item))
        {
            return;
        }

        if (this.Building is ShippingBin && item == Game1.getFarm().lastItemShipped)
        {
            Game1.getFarm().lastItemShipped = this.Items.LastOrDefault();
        }

        this.Items.RemoveEmptySlots();
        this.ShowMenu();
    }
}