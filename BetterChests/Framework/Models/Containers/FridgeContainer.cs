namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class FridgeContainer : BaseContainer<GameLocation>
{
    private readonly Chest chest;

    /// <summary>Initializes a new instance of the <see cref="FridgeContainer" /> class.</summary>
    /// <param name="location">The game location where the fridge storage is located.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public FridgeContainer(GameLocation location, Chest chest)
        : base(location)
    {
        this.chest = chest;
        this.InitOptions();
    }

    /// <inheritdoc />
    public override int Capacity => this.chest.GetActualCapacity();

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override IInventory Items => this.chest.GetItemsForPlayer();

    /// <inheritdoc />
    public override GameLocation Location =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(FridgeContainer));

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Location.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.chest.GetMutex();

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Location.GetFridgePosition()?.ToVector2() ?? Vector2.Zero;

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false)
    {
        var oldID = Game1.activeClickableMenu?.currentlySnappedComponent?.myID ?? -1;
        Game1.activeClickableMenu = this.GetItemGrabMenu(playSound, sourceItem: null, context: this.chest);
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
        remaining = this.chest.addItem(item);
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