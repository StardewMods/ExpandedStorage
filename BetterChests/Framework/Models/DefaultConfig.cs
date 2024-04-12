namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions { get; set; } = new();

    /// <inheritdoc />
    public int CarryChestLimit { get; set; } = 3;

    /// <inheritdoc />
    public int CarryChestSlowLimit { get; set; } = 1;

    /// <inheritdoc />
    public Controls Controls { get; set; } = new();

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance { get; set; } = -1;

    /// <inheritdoc />
    public int HslColorPickerHueSteps { get; set; } = 29;

    /// <inheritdoc />
    public int HslColorPickerSaturationSteps { get; set; } = 16;

    /// <inheritdoc />
    public int HslColorPickerLightnessSteps { get; set; } = 16;

    /// <inheritdoc />
    public FilterMethod InventoryTabMethod { get; set; } = FilterMethod.Hidden;

    /// <inheritdoc />
    public FeatureOption LockItem { get; set; }

    /// <inheritdoc />
    public bool LockItemHold { get; set; } = true;

    /// <inheritdoc />
    public FilterMethod SearchItemsMethod { get; set; } = FilterMethod.GrayedOut;

    /// <inheritdoc />
    public char SearchTagSymbol { get; set; } = '#';

    /// <inheritdoc />
    public char SearchNegationSymbol { get; set; } = '!';

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CarryChestLimit)}: {this.CarryChestLimit}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CarryChestSlowLimit)}: {this.CarryChestSlowLimit}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CraftFromChestDisableLocations)}: {string.Join(", ", this.CraftFromChestDisableLocations)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CraftFromWorkbench)}: {this.CraftFromWorkbench}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CraftFromWorkbenchDistance)}: {this.CraftFromWorkbenchDistance}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerHueSteps)}: {this.HslColorPickerHueSteps}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerSaturationSteps)}: {this.HslColorPickerSaturationSteps}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerLightnessSteps)}: {this.HslColorPickerLightnessSteps}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.InventoryTabMethod)}: {this.InventoryTabMethod}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.LockItem)}: {this.LockItem}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.LockItemHold)}: {this.LockItemHold}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.SearchItemsMethod)}: {this.SearchItemsMethod}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.SearchTagSymbol)}: {this.SearchTagSymbol}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.SearchNegationSymbol)}: {this.SearchNegationSymbol}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.StashToChestDisableLocations)}: {string.Join(", ", this.StashToChestDisableLocations)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.DefaultOptions)}: {this.DefaultOptions}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.Controls)}: {this.Controls}");

        return sb.ToString();
    }
}