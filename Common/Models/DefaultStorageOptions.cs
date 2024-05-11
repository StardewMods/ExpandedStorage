namespace StardewMods.Common.Models;

using System.Globalization;
using System.Text;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <inheritdoc />
internal class DefaultStorageOptions : IStorageOptions
{
    /// <inheritdoc />
    public string Description => string.Empty;

    /// <inheritdoc />
    public string DisplayName => string.Empty;

    /// <inheritdoc />
    public RangeOption AccessChest { get; set; } = RangeOption.Default;

    /// <inheritdoc />
    public int AccessChestPriority { get; set; }

    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CategorizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public string CategorizeChestSearchTerm { get; set; } = string.Empty;

    /// <inheritdoc />
    public FeatureOption ChestFinder { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CollectItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption ConfigureChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public RangeOption CookFromChest { get; set; } = RangeOption.Default;

    /// <inheritdoc />
    public RangeOption CraftFromChest { get; set; } = RangeOption.Default;

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; }

    /// <inheritdoc />
    public FeatureOption HslColorPicker { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption InventoryTabs { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public ChestMenuOption ResizeChest { get; set; } = ChestMenuOption.Default;

    /// <inheritdoc />
    public int ResizeChestCapacity { get; set; }

    /// <inheritdoc />
    public FeatureOption SearchItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption ShopFromChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption SortInventory { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public string SortInventoryBy { get; set; } = string.Empty;

    /// <inheritdoc />
    public RangeOption StashToChest { get; set; } = RangeOption.Default;

    /// <inheritdoc />
    public int StashToChestDistance { get; set; }

    /// <inheritdoc />
    public StashPriority StashToChestPriority { get; set; }

    /// <inheritdoc />
    public string StorageIcon { get; set; } = string.Empty;

    /// <inheritdoc />
    public FeatureOption StorageInfo { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption StorageInfoHover { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public string StorageName { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        this.ForEachOption(
            (name, option) =>
            {
                switch (option)
                {
                    case FeatureOption featureOption:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {featureOption.ToStringFast()}");
                        break;

                    case RangeOption rangeOption:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {rangeOption.ToStringFast()}");
                        break;

                    case ChestMenuOption chestMenuOption:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {chestMenuOption.ToStringFast()}");
                        break;

                    case StashPriority stashPriority:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {stashPriority.ToStringFast()}");
                        break;

                    case string stringValue:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {stringValue}");
                        break;

                    case int intValue:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {intValue}");
                        break;
                }
            });

        return sb.ToString();
    }
}