namespace StardewMods.Common.Services.Integrations.BetterChests;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Data;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterChests.IStorageOptions" />
internal class StorageOptions : DictionaryDataModel, IStorageOptions
{
    /// <summary>Initializes a new instance of the <see cref="StorageOptions" /> class.</summary>
    /// <param name="dictionaryModel">The backing dictionary.</param>
    public StorageOptions(IDictionaryModel dictionaryModel)
        : base(dictionaryModel) { }

    /// <inheritdoc />
    public virtual string Description => string.Empty;

    /// <inheritdoc />
    public virtual string DisplayName => string.Empty;

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.Get(nameof(this.AccessChest), StorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.AccessChest), value, StorageOptions.RangeOptionToString);
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
        get => this.Get(nameof(this.AutoOrganize), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.AutoOrganize), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(nameof(this.CarryChest), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CarryChest), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(nameof(this.CategorizeChest), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CategorizeChest), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.Get(nameof(this.CategorizeChestBlockItems), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CategorizeChestBlockItems), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.Get(nameof(this.CategorizeChestIncludeStacks), StorageOptions.StringToFeatureOption);
        set =>
            this.Set(nameof(this.CategorizeChestIncludeStacks), value, StorageOptions.FeatureOptionToString);
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
        get => this.Get(nameof(this.ChestFinder), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.ChestFinder), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(nameof(this.CollectItems), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.CollectItems), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(nameof(this.ConfigureChest), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.ConfigureChest), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.Get(nameof(this.CookFromChest), StorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.CookFromChest), value, StorageOptions.RangeOptionToString);
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(nameof(this.CraftFromChest), StorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.CraftFromChest), value, StorageOptions.RangeOptionToString);
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
        get => this.Get(nameof(this.HslColorPicker), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.HslColorPicker), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(nameof(this.InventoryTabs), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.InventoryTabs), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(nameof(this.OpenHeldChest), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.OpenHeldChest), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(nameof(this.ResizeChest), StorageOptions.StringToChestMenuOption);
        set => this.Set(nameof(this.ResizeChest), value, StorageOptions.ChestMenuOptionToString);
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
        get => this.Get(nameof(this.SearchItems), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.SearchItems), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(nameof(this.ShopFromChest), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.ShopFromChest), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption SortInventory
    {
        get => this.Get(nameof(this.SortInventory), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.SortInventory), value, StorageOptions.FeatureOptionToString);
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
        get => this.Get(nameof(this.StashToChest), StorageOptions.StringToRangeOption);
        set => this.Set(nameof(this.StashToChest), value, StorageOptions.RangeOptionToString);
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
        get => this.Get(nameof(this.StashToChestPriority), StorageOptions.StringToStashPriority);
        set => this.Set(nameof(this.StashToChestPriority), value, StorageOptions.StashPriorityToString);
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
        get => this.Get(nameof(this.StorageInfo), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.StorageInfo), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfoHover
    {
        get => this.Get(nameof(this.StorageInfoHover), StorageOptions.StringToFeatureOption);
        set => this.Set(nameof(this.StorageInfoHover), value, StorageOptions.FeatureOptionToString);
    }

    /// <inheritdoc />
    public string StorageName
    {
        get => this.Get(nameof(this.StorageName));
        set => this.Set(nameof(this.StorageName), value);
    }

    /// <inheritdoc />
    protected override string Prefix => "furyx639.BetterChests/";

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