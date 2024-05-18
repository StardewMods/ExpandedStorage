namespace StardewMods.BetterChests.Framework;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;

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

    /// <summary>Retrieves an internal icon.</summary>
    /// <param name="iconRegistry">The icon registry.</param>
    /// <param name="internalIcon">The internal icon.</param>
    /// <returns>Returns the icon.</returns>
    public static IIcon RequireIcon(this IIconRegistry iconRegistry, InternalIcon internalIcon) =>
        iconRegistry.RequireIcon(internalIcon.ToStringFast());

    /// <summary>Attempt to retrieve a specific internal icon.</summary>
    /// <param name="iconRegistry">The icon registry.</param>
    /// <param name="internalIcon">The internal icon.</param>
    /// <param name="icon">When this method returns, contains the icon; otherwise, null.</param>
    /// <returns><c>true</c> if the icon exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetIcon(
        this IIconRegistry iconRegistry,
        InternalIcon internalIcon,
        [NotNullWhen(true)] out IIcon? icon) =>
        iconRegistry.TryGetIcon(internalIcon.ToStringFast(), out icon);
}