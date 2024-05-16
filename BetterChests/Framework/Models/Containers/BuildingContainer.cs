namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests;
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
    /// <param name="building">The building to which the storage is connected.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public BuildingContainer(Building building, Chest chest)
        : base(building)
    {
        this.chest = chest;
        this.InitOptions();
    }

    /// <summary>Initializes a new instance of the <see cref="BuildingContainer" /> class.</summary>
    /// <param name="building">The building to which the storage is connected.</param>
    public BuildingContainer(Building building)
        : base(building) =>
        this.InitOptions();

    /// <summary>Gets the source building of the container.</summary>
    public Building Building =>
        this.Source.TryGetTarget(out var target)
            ? target
            : throw new ObjectDisposedException(nameof(BuildingContainer));

    /// <inheritdoc />
    public override int Capacity => this.chest?.GetActualCapacity() ?? int.MaxValue;

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override IInventory Items => this.chest?.GetItemsForPlayer() ?? Game1.getFarm().getShippingBin(Game1.player);

    /// <inheritdoc />
    public override GameLocation Location => this.Building.GetParentLocation();

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Building.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.chest?.GetMutex();

    /// <inheritdoc />
    public override Vector2 TileLocation =>
        new(
            this.Building.tileX.Value + (this.Building.tilesWide.Value / 2f),
            this.Building.tileY.Value + (this.Building.tilesHigh.Value / 2f));

    /// <inheritdoc />
    public override void GrabItemFromChest(Item? item, Farmer who)
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

    /// <inheritdoc />
    public override void GrabItemFromInventory(Item? item, Farmer who)
    {
        if (this.Building is not ShippingBin)
        {
            base.GrabItemFromInventory(item, who);
            return;
        }

        Game1.playSound("Ship");
        base.GrabItemFromInventory(item, who);
    }

    /// <inheritdoc />
    public override bool HighlightItems(Item? item) =>
        this.Building is ShippingBin ? Utility.highlightShippableObjects(item) : base.HighlightItems(item);

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false)
    {
        var itemGrabMenu = this.Building switch
        {
            // Vanilla Shipping Bin
            ShippingBin when this.ResizeChest is ChestMenuOption.Disabled => this.GetItemGrabMenu(
                reverseGrab: true,
                showReceivingMenu: false,
                snapToBottom: true,
                playRightClickSound: false,
                showOrganizeButton: false,
                source: ItemGrabMenu.source_none),

            // Shipping Bin with Chest Menu
            ShippingBin => this.GetItemGrabMenu(source: ItemGrabMenu.source_none, context: Game1.getFarm()),

            // Junimo Hut
            JunimoHut => this.GetItemGrabMenu(whichSpecialButton: ItemGrabMenu.specialButton_junimotoggle),
            _ => null,
        };

        if (itemGrabMenu is null)
        {
            base.ShowMenu(playSound);
            return;
        }

        var oldID = Game1.activeClickableMenu?.currentlySnappedComponent?.myID ?? -1;
        if (this.Building is not ShippingBin)
        {
            Game1.activeClickableMenu = itemGrabMenu;
            if (oldID == -1)
            {
                return;
            }

            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
            return;
        }

        if (this.ResizeChest is ChestMenuOption.Disabled)
        {
            itemGrabMenu.initializeUpperRightCloseButton();
            itemGrabMenu.setBackgroundTransparency(b: false);
            itemGrabMenu.setDestroyItemOnClick(b: true);
            itemGrabMenu.initializeShippingBin();
        }

        itemGrabMenu.inventory.moveItemSound = "Ship";
        Game1.activeClickableMenu = itemGrabMenu;
        if (oldID == -1)
        {
            return;
        }

        Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
        Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
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
    protected override void InitOptions()
    {
        base.InitOptions();

        if (this.Building is ShippingBin && string.IsNullOrWhiteSpace(this.StorageIcon))
        {
            this.StorageIcon = "StardewValley.Vanilla/ShippingBin";
        }
    }
}