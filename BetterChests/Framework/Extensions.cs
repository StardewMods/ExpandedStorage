namespace StardewMods.BetterChests.Framework;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Extension methods for Better Chests.</summary>
internal static class Extensions
{
    /// <summary>Executes the specified action for each config in the class.</summary>
    /// <param name="config">The config.</param>
    /// <param name="action">The action to be performed for each config.</param>
    public static void ForEachConfig(this IModConfig config, Action<string, object> action)
    {
        action(nameof(config.AccessChestsShowArrows), config.AccessChestsShowArrows);
        action(nameof(config.CarryChestLimit), config.CarryChestLimit);
        action(nameof(config.CarryChestSlowAmount), config.CarryChestSlowAmount);
        action(nameof(config.CarryChestSlowLimit), config.CarryChestSlowLimit);
        action(nameof(config.CraftFromChestDisableLocations), config.CraftFromChestDisableLocations);
        action(nameof(config.DebugMode), config.DebugMode);
        action(nameof(config.HslColorPickerHueSteps), config.HslColorPickerHueSteps);
        action(nameof(config.HslColorPickerSaturationSteps), config.HslColorPickerSaturationSteps);
        action(nameof(config.HslColorPickerLightnessSteps), config.HslColorPickerLightnessSteps);
        action(nameof(config.HslColorPickerPlacement), config.HslColorPickerPlacement);
        action(nameof(config.InventoryTabList), config.InventoryTabList);
        action(nameof(config.LockItem), config.LockItem);
        action(nameof(config.LockItemHold), config.LockItemHold);
        action(nameof(config.SearchItemsMethod), config.SearchItemsMethod);
        action(nameof(config.StashToChestDisableLocations), config.StashToChestDisableLocations);
        action(nameof(config.StorageInfoHoverItems), config.StorageInfoHoverItems);
        action(nameof(config.StorageInfoMenuItems), config.StorageInfoMenuItems);
        action(nameof(config.Controls), config.Controls);
        action(nameof(config.DefaultOptions), config.DefaultOptions);
        action(nameof(config.StorageOptions), config.StorageOptions);
    }
}