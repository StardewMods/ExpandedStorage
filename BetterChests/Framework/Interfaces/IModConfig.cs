namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <summary>Mod config data for Better Chests.</summary>
internal interface IModConfig
{
    /// <summary>Gets the default storage options.</summary>
    public DefaultStorageOptions DefaultOptions { get; }

    /// <summary>Gets the default options for different storage types.</summary>
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions { get; }

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

    /// <summary>
    /// Gets a value indicating if the chest cannot be remotely crafted from while the player is in one of the listed
    /// locations.
    /// </summary>
    public HashSet<string> CraftFromChestDisableLocations { get; }

    /// <summary>Gets a value for the number of steps in the hue color picker.</summary>
    public int HslColorPickerHueSteps { get; }

    /// <summary>Gets a value for the number of steps in the hue color picker.</summary>
    public int HslColorPickerSaturationSteps { get; }

    /// <summary>Gets a value for the number of steps in the hue color picker.</summary>
    public int HslColorPickerLightnessSteps { get; }

    /// <summary>Gets a value indicating whether the slot lock feature is enabled.</summary>
    public FeatureOption LockItem { get; }

    /// <summary>Gets a value indicating whether the slot lock button needs to be held down.</summary>
    public bool LockItemHold { get; }

    /// <summary>Gets a value indicating how searched items will be displayed.</summary>
    public FilterMethod SearchItemsMethod { get; }

    /// <summary>
    /// Gets a value indicating if the chest cannot be remotely crafted from while the player is in one of the listed
    /// locations.
    /// </summary>
    public HashSet<string> StashToChestDisableLocations { get; }
}