namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class NpcContainer : BaseContainer<NPC>
{
    private readonly Chest chest;

    /// <summary>Initializes a new instance of the <see cref="NpcContainer" /> class.</summary>
    /// <param name="npc">The npc to which the storage is connected.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public NpcContainer(NPC npc, Chest chest)
        : base(npc)
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
    public override GameLocation Location => this.Npc.currentLocation;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Npc.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.chest.GetMutex();

    /// <summary>Gets the source NPC of the container.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the NPC is disposed.</exception>
    public NPC Npc =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(NpcContainer));

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Npc.Tile;

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