namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewValley.Menus;

/// <summary>Mod config data for Better Chests.</summary>
internal interface IModConfig
{
    /// <summary>Gets a value indicating whether to show arrows when accessing chests.</summary>
    public bool AccessChestsShowArrows { get; }

    /// <summary>Gets a value indicating how many chests can be carried at once.</summary>
    public int CarryChestLimit { get; }

    /// <summary>Gets a value indicating the speed penalty for carrying chests above the limit.</summary>
    public float CarryChestSlowAmount { get; }

    /// <summary>Gets a value indicating how many chests can be carried before applying a slowness effect.</summary>
    public int CarryChestSlowLimit { get; }

    /// <summary>Gets the control scheme.</summary>
    public Controls Controls { get; }

    /// <summary>Gets the locations that crafting from storages will be disabled from.</summary>
    public HashSet<string> CraftFromChestDisableLocations { get; }

    /// <summary>Gets a value indicating whether debug mode will be enabled.</summary>
    public bool DebugMode { get; }

    /// <summary>Gets the default storage options.</summary>
    public DefaultStorageOptions DefaultOptions { get; }

    /// <summary>Gets a value for the number of steps in the hue color picker.</summary>
    public int HslColorPickerHueSteps { get; }

    /// <summary>Gets a value for the number of steps in the hue color picker.</summary>
    public int HslColorPickerLightnessSteps { get; }

    /// <summary>Gets the placement for the Hsl Color Picker.</summary>
    public InventoryMenu.BorderSide HslColorPickerPlacement { get; }

    /// <summary>Gets a value for the number of steps in the hue color picker.</summary>
    public int HslColorPickerSaturationSteps { get; }

    /// <summary>Gets the inventory tabs.</summary>
    public List<TabData> InventoryTabList { get; }

    /// <summary>Gets a value indicating whether the slot lock feature is enabled.</summary>
    public FeatureOption LockItem { get; }

    /// <summary>Gets a value indicating whether the slot lock button needs to be held down.</summary>
    public bool LockItemHold { get; }

    /// <summary>Gets a value indicating how searched items will be displayed.</summary>
    public FilterMethod SearchItemsMethod { get; }

    /// <summary>Gets the locations that stashing into storages will be disabled from.</summary>
    public HashSet<string> StashToChestDisableLocations { get; }

    /// <summary>Gets the info that will be displayed for storages that are being hovered over.</summary>
    public HashSet<StorageInfoItem> StorageInfoHoverItems { get; }

    /// <summary>Gets the info that will be displayed for storages from the inventory menu.</summary>
    public HashSet<StorageInfoItem> StorageInfoMenuItems { get; }

    /// <summary>Gets the default options for different storage types.</summary>
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions { get; }
}