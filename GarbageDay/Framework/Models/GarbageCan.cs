namespace StardewMods.GarbageDay.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Objects;

/// <summary>Encapsulates logic for each Garbage Can managed by this mod.</summary>
internal sealed class GarbageCan
{
    private readonly Chest chest;

    private bool checkedToday;
    private bool doubleMega;
    private bool dropQiBeans;
    private bool mega;
    private Item? specialItem;

    /// <summary>Initializes a new instance of the <see cref="GarbageCan" /> class.</summary>
    /// <param name="chest">A unique name given to the garbage can for its loot table.</param>
    public GarbageCan(Chest chest) => this.chest = chest;

    /// <summary>Gets the Location where the garbage can is placed.</summary>
    public GameLocation Location => this.chest.Location;

    /// <summary>Gets the tile of the Garbage Can.</summary>
    public Vector2 Tile => this.chest.TileLocation;

    private IInventory Items => this.chest.GetItemsForPlayer();

    private ModDataDictionary ModData => this.chest.modData;

    /// <summary>Adds an item to the garbage can determined by luck and mirroring vanilla chances.</summary>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="overrideItem">Manually override the item.</param>
    public void AddLoot(ILog log, Item? overrideItem = null)
    {
        // Reset daily state
        this.checkedToday = false;
        this.dropQiBeans = false;
        this.doubleMega = false;
        this.mega = false;
        if (!this.ModData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan))
        {
            return;
        }

        log.Trace("Adding loot item to garbage can {0}.", whichCan);

        if (overrideItem is not null)
        {
            log.Trace("Special loot item selected {0}", overrideItem.Name);
            this.specialItem = overrideItem;
            return;
        }

        this.Location.TryGetGarbageItem(
            whichCan,
            Game1.player.DailyLuck,
            out var item,
            out var selected,
            out var garbageRandom);

        if (selected is null)
        {
            log.Trace("No loot item selected");
            return;
        }

        if (selected.ItemId == "(O)890")
        {
            log.Trace("Special loot item selected {0}", item.Name);
            this.dropQiBeans = true;
            this.specialItem = item;
            return;
        }

        this.doubleMega = selected.IsDoubleMegaSuccess;
        this.mega = !this.doubleMega && selected.IsMegaSuccess;
        if (selected.AddToInventoryDirectly)
        {
            log.Trace("Special loot item selected {0}", item.Name);
            this.specialItem = item;
            return;
        }

        // Add item
        log.Trace("Regular loot item selected {0}", item.Name);
        this.chest.addItem(item);

        // Update color
        var colors = this.Items.Select(ItemContextTagManager.GetColorFromTags).OfType<Color>().ToList();
        if (!colors.Any())
        {
            this.chest.playerChoiceColor.Value = Color.Gray;
            return;
        }

        var index = garbageRandom.Next(colors.Count);
        this.chest.playerChoiceColor.Value = colors[index];
    }

    /// <summary>Called when a player attempts to open the garbage can.</summary>
    public void CheckAction()
    {
        if (!this.checkedToday)
        {
            this.checkedToday = true;
            Game1.stats.Increment("trashCansChecked");
        }

        // Drop Item
        if (this.dropQiBeans)
        {
            var origin = Game1.tileSize * (this.Tile + new Vector2(0.5f, -1));
            Game1.createItemDebris(this.specialItem, origin, 2, this.Location, (int)origin.Y + Game1.tileSize);
            this.dropQiBeans = false;
            return;
        }

        // Give Hat
        if (this.doubleMega)
        {
            this.doubleMega = false;
            this.Location.playSound("explosion");
        }

        if (this.mega)
        {
            this.mega = false;
            this.Location.playSound("crit");
        }

        if (this.specialItem is null)
        {
            this
                .chest.GetMutex()
                .RequestLock(
                    () =>
                    {
                        Game1.playSound("trashcan");
                        this.chest.ShowMenu();
                    });

            return;
        }

        if (this.specialItem.ItemId == "(H)66")
        {
            this.chest.playerChoiceColor.Value = Color.Black; // Remove Lid
        }

        Game1.player.addItemByMenuIfNecessary(this.specialItem);
        this.specialItem = null;
    }

    /// <summary>Empties the trash of all items.</summary>
    public void EmptyTrash() => this.Items.Clear();
}