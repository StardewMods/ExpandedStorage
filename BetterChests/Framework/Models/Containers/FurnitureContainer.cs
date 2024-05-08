namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class FurnitureContainer : BaseContainer<StorageFurniture>
{
    /// <summary>Initializes a new instance of the <see cref="FurnitureContainer" /> class.</summary>
    /// <param name="furniture">The furniture storage of the container.</param>
    public FurnitureContainer(StorageFurniture furniture)
        : base(furniture)
    {
        this.Items = new HeldItemsWrapper(furniture);
        this.InitOptions();
    }

    /// <summary>Gets the source furniture of the container.</summary>
    public StorageFurniture Furniture =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(StorageFurniture));

    /// <inheritdoc />
    public override int Capacity => int.MaxValue;

    /// <inheritdoc />
    public override IInventory Items { get; }

    /// <inheritdoc />
    public override GameLocation Location => this.Furniture.Location;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Furniture.TileLocation;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Furniture.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.Furniture.mutex;

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false)
    {
        this.Furniture.ShowShopMenu();

        if (playSound)
        {
            Game1.playSound("dwop");
        }
    }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        switch (this.Furniture)
        {
            case FishTankFurniture fishTankFurniture:
                if (!fishTankFurniture.HasRoomForThisItem(item))
                {
                    remaining = item;
                    return false;
                }

                break;
        }

        var stack = item.Stack;
        remaining = this.Furniture.AddItem(item);
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

    /// <inheritdoc />
    public override void GrabItemFromInventory(Item? item, Farmer who)
    {
        if (this.Furniture is not FishTankFurniture)
        {
            base.GrabItemFromInventory(item, who);
            return;
        }

        Game1.playSound("dropItemInWater");
        base.GrabItemFromInventory(item, who);
    }

    /// <inheritdoc />
    public override bool HighlightItems(Item? item) =>
        this.Furniture switch
        {
            FishTankFurniture fishTankFurniture => fishTankFurniture.HasRoomForThisItem(item),
            _ => base.HighlightItems(item),
        };
}