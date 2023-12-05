﻿namespace StardewMods.BetterChests.Framework.StorageObjects;

using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="IStorageData" />
internal abstract class Storage : IStorageData
{
    /// <summary>Initializes a new instance of the <see cref="Storage" /> class.</summary>
    /// <param name="context">The source object.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    protected Storage(object context, object? source, Vector2 position)
    {
        this.Context = context;
        this.Source = source;
        this.Position = position;
        this.GetActualCapacity = this.DefaultGetActualCapacity;
    }

    /// <summary>Gets the actual capacity of the object's storage.</summary>
    public virtual int ActualCapacity => this.GetActualCapacity();

    /// <summary>Gets the context object.</summary>
    public object Context { get; }

    /// <summary>Gets or sets a get method that returns the object's actual capacity.</summary>
    public Func<int> GetActualCapacity { get; set; }

    /// <summary>Gets info about the Storage object.</summary>
    public string Info
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append("{ ");
            switch (this)
            {
                case ChestStorage:
                    sb.Append("Type: Chest");
                    break;
                case FridgeStorage:
                    sb.Append("Type: Fridge");
                    break;
                case JunimoHutStorage:
                    sb.Append("Type: JunimoHut");
                    break;
                case ObjectStorage:
                    sb.Append("Type: Object");
                    break;
                case ShippingBinStorage:
                    sb.Append("Type: ShippingBin");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(this.ChestLabel))
            {
                sb.Append(CultureInfo.InvariantCulture, $", Name: {this.ChestLabel}");
            }

            sb.Append(CultureInfo.InvariantCulture, $", Location: {this.Location.Name}");
            if (!this.Position.Equals(Vector2.Zero))
            {
                sb.Append(
                    CultureInfo.InvariantCulture,
                    $", Position: ({this.Position.X.ToString(CultureInfo.InvariantCulture)}");

                sb.Append(CultureInfo.InvariantCulture, $", {this.Position.Y.ToString(CultureInfo.InvariantCulture)})");
            }

            if (this.Source is Farmer farmer)
            {
                sb.Append(CultureInfo.InvariantCulture, $", Inventory: {farmer.Name}");
            }

            sb.Append(" }");
            return sb.ToString();
        }
    }

    /// <summary>Gets the items in the object's storage.</summary>
    public abstract IInventory Inventory { get; }

    /// <summary>Gets the context object's current location.</summary>
    public virtual GameLocation Location
    {
        get
        {
            var source = this.Source;
            while (source is Storage parent && !ReferenceEquals(this, parent))
            {
                source = parent.Source;
            }

            return source switch
            {
                GameLocation gameLocation => gameLocation,
                Character character => character.currentLocation,
                _ => Game1.currentLocation,
            };
        }
    }

    /// <summary>Gets the ModData associated with the context object.</summary>
    public abstract ModDataDictionary ModData { get; }

    /// <summary>Gets the mutex required to lock this object.</summary>
    public virtual NetMutex? Mutex => default;

    /// <summary>Gets the coordinate of this object.</summary>
    public Vector2 Position { get; }

    /// <summary>Gets the source context where this storage is contained.</summary>
    public object? Source { get; }

    /// <inheritdoc />
    public virtual FeatureOption AutoOrganize
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/AutoOrganize", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/AutoOrganize"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption CarryChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/CarryChest", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CarryChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption CarryChestSlow
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/CarryChestSlow", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CarryChestSlow"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption ChestInfo
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/ChestInfo", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/ChestInfo"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual string ChestLabel
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/ChestLabel", out var label) ? label : string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.ModData.Remove("furyx639.BetterChests/ChestLabel");
                return;
            }

            this.ModData["furyx639.BetterChests/ChestLabel"] = value;
        }
    }

    /// <inheritdoc />
    public virtual FeatureOption ChestMenuTabs
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/ChestMenuTabs", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/ChestMenuTabs"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual HashSet<string> ChestMenuTabSet
    {
        get =>
            new(
                this.ModData.TryGetValue("furyx639.BetterChests/ChestMenuTabSet", out var value)
                && !string.IsNullOrWhiteSpace(value)
                    ? value.Split(',')
                    : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/ChestMenuTabSet");
            }

            this.ModData["furyx639.BetterChests/ChestMenuTabSet"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public virtual FeatureOption CollectItems
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/CollectItems", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CollectItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption Configurator
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/Configurator", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/Configurator"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual InGameMenu ConfigureMenu
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/ConfigureMenu", out var value)
            && InGameMenuExtensions.TryParse(value, out var menu, true)
                ? menu
                : InGameMenu.Default;
        set => this.ModData["furyx639.BetterChests/ConfigureMenu"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOptionRange CraftFromChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/CraftFromChest", out var value)
            && FeatureOptionRangeExtensions.TryParse(value, out var range, true)
                ? range
                : FeatureOptionRange.Default;
        set => this.ModData["furyx639.BetterChests/CraftFromChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual HashSet<string> CraftFromChestDisableLocations
    {
        get =>
            new(
                this.ModData.TryGetValue("furyx639.BetterChests/CraftFromChestDisableLocations", out var value)
                && !string.IsNullOrWhiteSpace(value)
                    ? value.Split(',')
                    : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/CraftFromChestDisableLocations");
            }

            this.ModData["furyx639.BetterChests/CraftFromChestDisableLocations"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public virtual int CraftFromChestDistance
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/CraftFromChestDistance", out var value)
            && int.TryParse(value, out var distance)
                ? distance
                : 0;
        set =>
            this.ModData["furyx639.BetterChests/CraftFromChestDistance"] = value.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public virtual FeatureOption CustomColorPicker
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/CustomColorPicker", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CustomColorPicker"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption FilterItems
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/FilterItems", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/FilterItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual HashSet<string> FilterItemsList
    {
        get =>
            new(
                this.ModData.TryGetValue("furyx639.BetterChests/FilterItemsList", out var value)
                && !string.IsNullOrWhiteSpace(value)
                    ? value.Split(',')
                    : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/FilterItemsList");
            }

            this.ModData["furyx639.BetterChests/FilterItemsList"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public virtual FeatureOption HideItems
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/HideItems", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/HideItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption LabelChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/LabelChest", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/LabelChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption OpenHeldChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/OpenHeldChest", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/OpenHeldChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption OrganizeChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/OrganizeChest", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/OrganizeChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual GroupBy OrganizeChestGroupBy
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/OrganizeChestGroupBy", out var value)
            && GroupByExtensions.TryParse(value, out var groupBy, true)
                ? groupBy
                : GroupBy.Default;
        set => this.ModData["furyx639.BetterChests/OrganizeChestGroupBy"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual SortBy OrganizeChestSortBy
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/OrganizeChestSortBy", out var value)
            && SortByExtensions.TryParse(value, out var sortBy, true)
                ? sortBy
                : SortBy.Default;
        set => this.ModData["furyx639.BetterChests/OrganizeChestSortBy"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption ResizeChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/ResizeChest", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/ResizeChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual int ResizeChestCapacity
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/ResizeChestCapacity", out var value)
            && int.TryParse(value, out var capacity)
                ? capacity
                : 0;
        set => this.ModData["furyx639.BetterChests/ResizeChestCapacity"] = value.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public virtual FeatureOption SearchItems
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/SearchItems", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/SearchItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOptionRange StashToChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/StashToChest", out var value)
            && FeatureOptionRangeExtensions.TryParse(value, out var range, true)
                ? range
                : FeatureOptionRange.Default;
        set => this.ModData["furyx639.BetterChests/StashToChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual HashSet<string> StashToChestDisableLocations
    {
        get =>
            new(
                this.ModData.TryGetValue("furyx639.BetterChests/StashToChestDisableLocations", out var value)
                && !string.IsNullOrWhiteSpace(value)
                    ? value.Split(',')
                    : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/StashToChestDisableLocations");
            }

            this.ModData["furyx639.BetterChests/StashToChestDisableLocations"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public virtual int StashToChestDistance
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/StashToChestDistance", out var value)
            && int.TryParse(value, out var distance)
                ? distance
                : 0;
        set =>
            this.ModData["furyx639.BetterChests/StashToChestDistance"] = value.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public virtual int StashToChestPriority
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/StashToChestPriority", out var value)
            && int.TryParse(value, out var priority)
                ? priority
                : 0;
        set =>
            this.ModData["furyx639.BetterChests/StashToChestPriority"] = value.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public virtual FeatureOption StashToChestStacks
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/StashToChestStacks", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/StashToChestStacks"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption TransferItems
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/TransferItems", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/TransferItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption UnloadChest
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/UnloadChest", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/UnloadChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public virtual FeatureOption UnloadChestCombine
    {
        get =>
            this.ModData.TryGetValue("furyx639.BetterChests/UnloadChestCombine", out var value)
            && FeatureOptionExtensions.TryParse(value, out var option, true)
                ? option
                : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/UnloadChestCombine"] = value.ToStringFast();
    }

    /// <summary>Attempts to add an item into the storage.</summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be added completely, or null if it could.</returns>
    public virtual Item? AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Inventory)
        {
            if (existingItem is null || !item.canStackWith(existingItem))
            {
                continue;
            }

            item.Stack = existingItem.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Inventory.Count >= this.ActualCapacity)
        {
            return item;
        }

        this.Inventory.Add(item);
        return null;
    }

    /// <summary>Removes null items from the storage.</summary>
    public void ClearNulls()
    {
        for (var index = this.Inventory.Count - 1; index >= 0; --index)
        {
            if (this.Inventory[index] is null)
            {
                this.Inventory.RemoveAt(index);
            }
        }
    }

    /// <summary>Creates an <see cref="ItemGrabMenu" /> for this storage container.</summary>
    public virtual void ShowMenu()
    {
        var menu = new ItemGrabMenu(
            this.Inventory,
            false,
            true,
            InventoryMenu.highlightAllItems,
            this.GrabInventoryItem,
            null,
            this.GrabStorageItem,
            false,
            true,
            true,
            true,
            true,
            0,
            null,
            -1,
            this.Context);

        if (Game1.options.SnappyMenus
            && Game1.activeClickableMenu is ItemGrabMenu
            {
                currentlySnappedComponent:
                { } currentlySnappedComponent,
            })
        {
            menu.setCurrentlySnappedComponentTo(currentlySnappedComponent.myID);
            menu.snapCursorToCurrentSnappedComponent();
        }

        Game1.activeClickableMenu = menu;
    }

    /// <summary>Grabs an item from a player into this storage container.</summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player whose inventory to grab the item from.</param>
    protected void GrabInventoryItem(Item item, Farmer who)
    {
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        var tmp = this.AddItem(item);
        if (tmp == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            tmp = who.addItemToInventory(tmp);
        }

        this.ClearNulls();
        var oldId = Game1.activeClickableMenu.currentlySnappedComponent?.myID ?? -1;
        this.ShowMenu();
        ((ItemGrabMenu)Game1.activeClickableMenu).heldItem = tmp;
        if (oldId == -1)
        {
            return;
        }

        Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldId);
        Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
    }

    /// <summary>Grab an item from this storage container.</summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player grabbing the item.</param>
    protected void GrabStorageItem(Item item, Farmer who)
    {
        if (!who.couldInventoryAcceptThisItem(item))
        {
            return;
        }

        this.Inventory.Remove(item);
        this.ClearNulls();
        this.ShowMenu();
    }

    private int DefaultGetActualCapacity() =>
        this.ResizeChestCapacity switch
        {
            < 0 => int.MaxValue,
            > 0 => this.ResizeChestCapacity,
            _ => Chest.capacity,
        };
}
