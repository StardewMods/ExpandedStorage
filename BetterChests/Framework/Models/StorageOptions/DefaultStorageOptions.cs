namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using System.Globalization;
using System.Text;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal class DefaultStorageOptions : IStorageOptions
{
    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption CategorizeChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption CategorizeChestAutomatically { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FilterMethod CategorizeChestMethod { get; set; } = FilterMethod.GrayedOut;

    /// <inheritdoc />
    public HashSet<string> CategorizeChestTags { get; set; } = [];

    /// <inheritdoc />
    public FeatureOption ChestFinder { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption ChestInfo { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption CollectItems { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption ConfigureChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public RangeOption CraftFromChest { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; } = -1;

    /// <inheritdoc />
    public FeatureOption HslColorPicker { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption InventoryTabs { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public HashSet<string> InventoryTabList { get; set; } =
    [
        "Clothing", "Cooking", "Crops", "Equipment", "Fishing", "Materials", "Misc", "Seeds",
    ];

    /// <inheritdoc />
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public ChestMenuOption ResizeChest { get; set; } = ChestMenuOption.Large;

    /// <inheritdoc />
    public int ResizeChestCapacity { get; set; }

    /// <inheritdoc />
    public FeatureOption SearchItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption ShopFromChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public RangeOption StashToChest { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public int StashToChestDistance { get; set; } = 10;

    /// <inheritdoc />
    public int StashToChestPriority { get; set; }

    /// <inheritdoc />
    public virtual string GetDescription() => I18n.Storage_Other_Tooltip();

    /// <inheritdoc />
    public virtual string GetDisplayName() => I18n.Storage_Other_Name();

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine(CultureInfo.InvariantCulture, $"Display Name: {this.GetDisplayName()}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.AutoOrganize)}: {this.AutoOrganize}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CarryChest)}: {this.CarryChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CategorizeChest)}: {this.CategorizeChest}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CategorizeChestAutomatically)}: {this.CategorizeChestAutomatically}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CategorizeChestMethod)}: {this.CategorizeChestMethod}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CategorizeChestTags)}: {string.Join(", ", this.CategorizeChestTags)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ChestFinder)}: {this.ChestFinder}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ChestInfo)}: {this.ChestInfo}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CollectItems)}: {this.CollectItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ConfigureChest)}: {this.ConfigureChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CraftFromChest)}: {this.CraftFromChest}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CraftFromChestDistance)}: {this.CraftFromChestDistance}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.HslColorPicker)}: {this.HslColorPicker}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.InventoryTabs)}: {this.InventoryTabs}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.InventoryTabList)}: {string.Join(", ", this.InventoryTabList)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.OpenHeldChest)}: {this.OpenHeldChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ResizeChest)}: {this.ResizeChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ResizeChestCapacity)}: {this.ResizeChestCapacity}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.SearchItems)}: {this.SearchItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ShopFromChest)}: {this.ShopFromChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.StashToChest)}: {this.StashToChest}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.StashToChestDistance)}: {this.StashToChestDistance}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.StashToChestPriority)}: {this.StashToChestPriority}");

        return sb.ToString();
    }
}