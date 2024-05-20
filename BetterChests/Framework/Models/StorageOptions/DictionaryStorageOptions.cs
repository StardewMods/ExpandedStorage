namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterChests.IStorageOptions" />
internal sealed class DictionaryStorageOptions : DictionaryDataModel, IStorageOptions
{
    private readonly Func<string> getDescription;
    private readonly Func<string> getDisplayName;

    /// <summary>Initializes a new instance of the <see cref="DictionaryStorageOptions" /> class.</summary>
    /// <param name="dictionaryModel">The backing dictionary.</param>
    /// <param name="getDescription">Get method for the description.</param>
    /// <param name="getDisplayName">Get method for the display name.</param>
    public DictionaryStorageOptions(
        IDictionaryModel dictionaryModel,
        Func<string>? getDescription = null,
        Func<string>? getDisplayName = null)
        : base(dictionaryModel)
    {
        this.getDescription = getDescription ?? I18n.Storage_Other_Tooltip;
        this.getDisplayName = getDisplayName ?? I18n.Storage_Other_Name;
    }

    /// <inheritdoc />
    public string Description => this.getDescription();

    /// <inheritdoc />
    public string DisplayName => this.getDisplayName();

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.Get(nameof(this.AccessChest), DictionaryStorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.AccessChest), value, DictionaryStorageOptions.RangeOptionToString);
    }

    /// <inheritdoc />
    public int AccessChestPriority
    {
        get => this.Get(nameof(this.AccessChestPriority), DictionaryDataModel.StringToInt);
        set => this.Set(nameof(this.AccessChestPriority), value, DictionaryDataModel.IntToString);
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(nameof(this.AutoOrganize), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.AutoOrganize), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(nameof(this.CarryChest), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CarryChest), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(nameof(this.CategorizeChest), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CategorizeChest), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.Get(nameof(this.CategorizeChestBlockItems), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CategorizeChestBlockItems), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.Get(nameof(this.CategorizeChestIncludeStacks), DictionaryStorageOptions.StringToFeatureOption);
        set =>
            this.Set(nameof(this.CategorizeChestIncludeStacks), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public string CategorizeChestSearchTerm
    {
        get => this.Get(nameof(this.CategorizeChestSearchTerm));
        set => this.Set(nameof(this.CategorizeChestSearchTerm), value);
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(nameof(this.ChestFinder), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.ChestFinder), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(nameof(this.CollectItems), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CollectItems), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(nameof(this.ConfigureChest), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.ConfigureChest), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.Get(nameof(this.CookFromChest), DictionaryStorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.CookFromChest), value, DictionaryStorageOptions.RangeOptionToString);
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(nameof(this.CraftFromChest), DictionaryStorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.CraftFromChest), value, DictionaryStorageOptions.RangeOptionToString);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Get(nameof(this.CraftFromChestDistance), DictionaryDataModel.StringToInt);
        set => this.Set(nameof(this.CraftFromChestDistance), value, DictionaryDataModel.IntToString);
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(nameof(this.HslColorPicker), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.HslColorPicker), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(nameof(this.InventoryTabs), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.InventoryTabs), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(nameof(this.OpenHeldChest), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.OpenHeldChest), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(nameof(this.ResizeChest), DictionaryStorageOptions.StringToChestMenuOption);
        set => this.Set(nameof(this.ResizeChest), value, DictionaryStorageOptions.ChestMenuOptionToString);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Get(nameof(this.ResizeChestCapacity), DictionaryDataModel.StringToInt);
        set => this.Set(nameof(this.ResizeChestCapacity), value, DictionaryDataModel.IntToString);
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(nameof(this.SearchItems), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.SearchItems), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(nameof(this.ShopFromChest), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.ShopFromChest), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption SortInventory
    {
        get => this.Get(nameof(this.SortInventory), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.SortInventory), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public string SortInventoryBy
    {
        get => this.Get(nameof(this.SortInventoryBy));
        set => this.Set(nameof(this.SortInventoryBy), value);
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(nameof(this.StashToChest), DictionaryStorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.StashToChest), value, DictionaryStorageOptions.RangeOptionToString);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Get(nameof(this.StashToChestDistance), DictionaryDataModel.StringToInt);
        set => this.Set(nameof(this.StashToChestDistance), value, DictionaryDataModel.IntToString);
    }

    /// <inheritdoc />
    public StashPriority StashToChestPriority
    {
        get => this.Get(nameof(this.StashToChestPriority), DictionaryStorageOptions.StringToStashPriority);
        set => this.Set(nameof(this.StashToChestPriority), value, DictionaryStorageOptions.StashPriorityToString);
    }

    /// <inheritdoc />
    public string StorageIcon
    {
        get => this.Get(nameof(this.StorageIcon));
        set => this.Set(nameof(this.StorageIcon), value);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfo
    {
        get => this.Get(nameof(this.StorageInfo), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.StorageInfo), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfoHover
    {
        get => this.Get(nameof(this.StorageInfoHover), DictionaryStorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.StorageInfoHover), value, DictionaryStorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public string StorageName
    {
        get => this.Get(nameof(this.StorageName));
        set => this.Set(nameof(this.StorageName), value);
    }

    /// <inheritdoc />
    protected override string Prefix => Mod.Prefix;

    private static string ChestMenuOptionToString(ChestMenuOption value) => value.ToStringFast();

    private static string FeatureOptionToString(FeatureOption value) => value.ToStringFast();

    private static string RangeOptionToString(RangeOption value) => value.ToStringFast();

    private static string StashPriorityToString(StashPriority value) => value.ToStringFast();

    private static ChestMenuOption StringToChestMenuOption(string value) =>
        ChestMenuOptionExtensions.TryParse(value, out var chestMenuOption) ? chestMenuOption : ChestMenuOption.Default;

    private static FeatureOption StringToFeatureOption(string value) =>
        FeatureOptionExtensions.TryParse(value, out var featureOption, true) ? featureOption : FeatureOption.Default;

    private static RangeOption StringToRangeOption(string value) =>
        RangeOptionExtensions.TryParse(value, out var rangeOption, true) ? rangeOption : RangeOption.Default;

    private static StashPriority StringToStashPriority(string value) =>
        StashPriorityExtensions.TryParse(value, out var stashPriority) ? stashPriority : StashPriority.Default;
}