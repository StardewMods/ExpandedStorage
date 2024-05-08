namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class ObjectContainer : BaseContainer<SObject>
{
    /// <summary>Initializes a new instance of the <see cref="ObjectContainer" /> class.</summary>
    /// <param name="obj">The storage object.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public ObjectContainer(SObject obj, Chest chest)
        : base(obj)
    {
        this.Chest = chest;
        this.InitOptions();
    }

    /// <inheritdoc />
    public override int Capacity => this.Chest.GetActualCapacity();

    /// <summary>Gets the source chest of the container.</summary>
    public Chest Chest { get; }

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override IInventory Items => this.Chest.GetItemsForPlayer();

    /// <inheritdoc />
    public override GameLocation Location => this.Object.Location;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Object.modData;

    /// <inheritdoc />
    public override NetMutex Mutex => this.Chest.GetMutex();

    /// <summary>Gets the source object of the container.</summary>
    public SObject Object =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(ObjectContainer));

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Object.TileLocation;

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
        if (!this.Items.HasAny())
        {
            this.Object.showNextIndex.Value = false;
        }

        return true;
    }
}