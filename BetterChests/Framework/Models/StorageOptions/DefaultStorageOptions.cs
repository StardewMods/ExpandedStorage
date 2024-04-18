namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using System.Globalization;
using System.Text;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal class DefaultStorageOptions : IStorageOptions
{
    /// <inheritdoc />
    public RangeOption AccessChest { get; set; } = RangeOption.Default;

    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CategorizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public string CategorizeChestSearchTerm { get; set; } = string.Empty;

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption ChestFinder { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption ChestInfo { get; set; } = FeatureOption.Default;

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
    public RangeOption StashToChest { get; set; } = RangeOption.Default;

    /// <inheritdoc />
    public int StashToChestDistance { get; set; }

    /// <inheritdoc />
    public StashPriority StashToChestPriority { get; set; }

    /// <inheritdoc />
    public string StorageName { get; set; } = string.Empty;

    /// <inheritdoc />
    public IStorageOptions GetActualOptions() => this;

    /// <inheritdoc />
    public IStorageOptions GetParentOptions() => this;

    /// <inheritdoc />
    public virtual string GetDescription() => I18n.Storage_Other_Tooltip();

    /// <inheritdoc />
    public virtual string GetDisplayName() => I18n.Storage_Other_Name();

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine(CultureInfo.InvariantCulture, $"Display Name: {this.GetDisplayName()}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.AccessChest)}: {this.AccessChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.AutoOrganize)}: {this.AutoOrganize}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CarryChest)}: {this.CarryChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CategorizeChest)}: {this.CategorizeChest}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CategorizeChestBlockItems)}: {this.CategorizeChestBlockItems}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CategorizeChestSearchTerm)}: {this.CategorizeChestSearchTerm}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CategorizeChestIncludeStacks)}: {this.CategorizeChestIncludeStacks}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ChestFinder)}: {this.ChestFinder}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ChestInfo)}: {this.ChestInfo}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CollectItems)}: {this.CollectItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ConfigureChest)}: {this.ConfigureChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CookFromChest)}: {this.CookFromChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CraftFromChest)}: {this.CraftFromChest}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CraftFromChestDistance)}: {this.CraftFromChestDistance}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.HslColorPicker)}: {this.HslColorPicker}");
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