namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using System.Globalization;
using NetEscapades.EnumGenerators;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <inheritdoc />
internal abstract class DictionaryStorageOptions : IStorageOptions
{
    private const string Prefix = "furyx639.BetterChests/";
    private readonly Dictionary<string, CachedValue<ChestMenuOption>> cachedChestMenuOption = new();

    private readonly Dictionary<string, CachedValue<int>> cachedInt = new();
    private readonly Dictionary<string, CachedValue<FeatureOption>> cachedOption = new();
    private readonly Dictionary<string, CachedValue<RangeOption>> cachedRangeOption = new();

    /// <inheritdoc />
    public virtual string Description => I18n.Storage_Other_Name();

    /// <inheritdoc />
    public virtual string DisplayName => I18n.Storage_Other_Tooltip();

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.Get(RangeOptionKey.AccessChest);
        set => this.Set(RangeOptionKey.AccessChest, value);
    }

    /// <inheritdoc />
    public int AccessChestPriority
    {
        get => this.Get(IntegerKey.AccessChestPriority);
        set => this.Set(IntegerKey.AccessChestPriority, value);
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(OptionKey.AutoOrganize);
        set => this.Set(OptionKey.AutoOrganize, value);
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(OptionKey.CarryChest);
        set => this.Set(OptionKey.CarryChest, value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(OptionKey.CategorizeChest);
        set => this.Set(OptionKey.CategorizeChest, value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.Get(OptionKey.CategorizeChestBlockItems);
        set => this.Set(OptionKey.CategorizeChestBlockItems, value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.Get(OptionKey.CategorizeChestIncludeStacks);
        set => this.Set(OptionKey.CategorizeChestIncludeStacks, value);
    }

    /// <inheritdoc />
    public string CategorizeChestSearchTerm
    {
        get => this.Get(StringKey.CategorizeChestSearchTerm);
        set => this.Set(StringKey.CategorizeChestSearchTerm, value);
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(OptionKey.ChestFinder);
        set => this.Set(OptionKey.ChestFinder, value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(OptionKey.CollectItems);
        set => this.Set(OptionKey.CollectItems, value);
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(OptionKey.ConfigureChest);
        set => this.Set(OptionKey.ConfigureChest, value);
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.Get(RangeOptionKey.CookFromChest);
        set => this.Set(RangeOptionKey.CookFromChest, value);
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(RangeOptionKey.CraftFromChest);
        set => this.Set(RangeOptionKey.CraftFromChest, value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Get(IntegerKey.CraftFromChestDistance);
        set => this.Set(IntegerKey.CraftFromChestDistance, value);
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(OptionKey.HslColorPicker);
        set => this.Set(OptionKey.HslColorPicker, value);
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(OptionKey.InventoryTabs);
        set => this.Set(OptionKey.InventoryTabs, value);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(OptionKey.OpenHeldChest);
        set => this.Set(OptionKey.OpenHeldChest, value);
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get =>
            ChestMenuOptionExtensions.TryParse(this.Get(StringKey.ResizeChest), out var capacityOption)
                ? capacityOption
                : ChestMenuOption.Default;
        set => this.Set(StringKey.ResizeChest, value == ChestMenuOption.Default ? string.Empty : value.ToStringFast());
    }

    /// <inheritdoc />
    public virtual int ResizeChestCapacity
    {
        get => this.Get(IntegerKey.ResizeChestCapacity);
        set => this.Set(IntegerKey.ResizeChestCapacity, value);
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(OptionKey.SearchItems);
        set => this.Set(OptionKey.SearchItems, value);
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(OptionKey.ShopFromChest);
        set => this.Set(OptionKey.ShopFromChest, value);
    }

    /// <inheritdoc />
    public FeatureOption SortInventory
    {
        get => this.Get(OptionKey.SortInventory);
        set => this.Set(OptionKey.SortInventory, value);
    }

    /// <inheritdoc />
    public string SortInventoryBy
    {
        get => this.Get(StringKey.SortInventoryBy);
        set => this.Set(StringKey.SortInventoryBy, value);
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(RangeOptionKey.StashToChest);
        set => this.Set(RangeOptionKey.StashToChest, value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Get(IntegerKey.StashToChestDistance);
        set => this.Set(IntegerKey.StashToChestDistance, value);
    }

    /// <inheritdoc />
    public StashPriority StashToChestPriority
    {
        get =>
            StashPriorityExtensions.TryParse(this.Get(StringKey.StashToChestPriority), out var stashPriority)
                ? stashPriority
                : StashPriority.Default;
        set => this.Set(StringKey.StashToChestPriority, value.ToStringFast());
    }

    /// <inheritdoc />
    public string StorageIcon
    {
        get => this.Get(StringKey.StorageIcon);
        set => this.Set(StringKey.StorageIcon, value);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfo
    {
        get => this.Get(OptionKey.StorageInfo);
        set => this.Set(OptionKey.StorageInfo, value);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfoHover
    {
        get => this.Get(OptionKey.StorageInfoHover);
        set => this.Set(OptionKey.StorageInfoHover, value);
    }

    /// <inheritdoc />
    public string StorageName
    {
        get => this.Get(StringKey.StorageName);
        set => this.Set(StringKey.StorageName, value);
    }

    /// <summary>Sets the value for a given key in the derived class.</summary>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to be set.</param>
    protected abstract void SetValue(string key, string value);

    /// <summary>Tries to get the data associated with the specified key.</summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key; otherwise, null.</param>
    /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
    protected abstract bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    private FeatureOption Get(OptionKey optionKey)
    {
        var key = DictionaryStorageOptions.Prefix + optionKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return FeatureOption.Default;
        }

        // Return from cache
        if (this.cachedOption.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default;
        this.cachedOption[key] = new CachedValue<FeatureOption>(value, newValue);
        return newValue;
    }

    private RangeOption Get(RangeOptionKey rangeOptionKey)
    {
        var key = DictionaryStorageOptions.Prefix + rangeOptionKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return RangeOption.Default;
        }

        // Return from cache
        if (this.cachedRangeOption.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = RangeOptionExtensions.TryParse(value, out var rangeOption) ? rangeOption : RangeOption.Default;
        this.cachedRangeOption[key] = new CachedValue<RangeOption>(value, newValue);
        return newValue;
    }

    private int Get(IntegerKey integerKey)
    {
        var key = DictionaryStorageOptions.Prefix + integerKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return 0;
        }

        // Return from cache
        if (this.cachedInt.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = value.GetInt();
        this.cachedInt[key] = new CachedValue<int>(value, newValue);
        return newValue;
    }

    private string Get(StringKey stringKey)
    {
        var key = DictionaryStorageOptions.Prefix + stringKey.ToStringFast();
        return !this.TryGetValue(key, out var value) ? string.Empty : value;
    }

    private void Set(OptionKey optionKey, FeatureOption value)
    {
        var key = DictionaryStorageOptions.Prefix + optionKey.ToStringFast();
        var stringValue = value == FeatureOption.Default ? string.Empty : value.ToStringFast();
        this.cachedOption[key] = new CachedValue<FeatureOption>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(RangeOptionKey rangeOptionKey, RangeOption value)
    {
        var key = DictionaryStorageOptions.Prefix + rangeOptionKey.ToStringFast();
        var stringValue = value == RangeOption.Default ? string.Empty : value.ToStringFast();
        this.cachedRangeOption[key] = new CachedValue<RangeOption>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(IntegerKey integerKey, int value)
    {
        var key = DictionaryStorageOptions.Prefix + integerKey.ToStringFast();
        var stringValue = value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);
        this.cachedInt[key] = new CachedValue<int>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(StringKey stringKey, string value)
    {
        var key = DictionaryStorageOptions.Prefix + stringKey.ToStringFast();
        this.SetValue(key, value);
    }

    private readonly struct CachedValue<T>(string originalValue, T cachedValue)
    {
        public T Value { get; } = cachedValue;

        public string OriginalValue { get; } = originalValue;
    }
#pragma warning disable SA1201
#pragma warning disable SA1600
#pragma warning disable SA1602

    [EnumExtensions]
    internal enum IntegerKey
    {
        AccessChestPriority,
        CraftFromChestDistance,
        ResizeChestCapacity,
        StashToChestDistance,
    }

    [EnumExtensions]
    internal enum OptionKey
    {
        AutoOrganize,
        CarryChest,
        CategorizeChest,
        CategorizeChestBlockItems,
        CategorizeChestIncludeStacks,
        ChestFinder,
        CollectItems,
        ConfigureChest,
        HslColorPicker,
        InventoryTabs,
        OpenHeldChest,
        SearchItems,
        ShopFromChest,
        SortInventory,
        StorageInfo,
        StorageInfoHover,
    }

    [EnumExtensions]
    internal enum RangeOptionKey
    {
        AccessChest,
        CookFromChest,
        CraftFromChest,
        StashToChest,
    }

    [EnumExtensions]
    internal enum StringKey
    {
        CategorizeChestSearchTerm,
        ResizeChest,
        SortInventoryBy,
        StashToChestPriority,
        StorageIcon,
        StorageName,
    }
}