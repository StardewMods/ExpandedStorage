namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class ObjectContainer : BaseContainer<SObject>
{
    /// <summary>Initializes a new instance of the <see cref="ObjectContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="obj">The storage object.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public ObjectContainer(IStorageOptions baseOptions, SObject obj, Chest chest)
        : base(baseOptions)
    {
        this.Source = new WeakReference<SObject>(obj);
        this.Chest = chest;
    }

    /// <summary>Gets the source object of the container.</summary>
    public SObject Object =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(ObjectContainer));

    /// <inheritdoc />
    public override int Capacity => this.Chest.GetActualCapacity();

    /// <summary>Gets the source chest of the container.</summary>
    public Chest Chest { get; }

    /// <inheritdoc />
    public override IInventory Items => this.Chest.GetItemsForPlayer();

    /// <inheritdoc />
    public override GameLocation Location => this.Object.Location;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Object.TileLocation;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Object.modData;

    /// <inheritdoc />
    public override NetMutex Mutex => this.Chest.GetMutex();

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<SObject> Source { get; }

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false)
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
            this.Object,
            -1,
            this.Object);
    }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        remaining = this.Chest.addItem(item);
        return remaining is null || remaining.Stack != stack;
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
}