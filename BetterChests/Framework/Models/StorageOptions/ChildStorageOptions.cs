namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal class ChildStorageOptions : IStorageOptions
{
    private readonly IStorageOptions child;
    private readonly Func<IStorageOptions> getParent;

    /// <summary>Initializes a new instance of the <see cref="ChildStorageOptions" /> class.</summary>
    /// <param name="getParent">Get the parent storage options.</param>
    /// <param name="child">The child storage options.</param>
    public ChildStorageOptions(Func<IStorageOptions> getParent, IStorageOptions child)
    {
        this.getParent = getParent;
        this.child = child;
    }

    private IStorageOptions ActualOptions => this.GetActualOptions();

    private IStorageOptions Parent => this.getParent();

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.Get(storage => storage.AccessChest);
        set => this.ActualOptions.AccessChest = value == this.Parent.AccessChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(storage => storage.AutoOrganize);
        set => this.ActualOptions.AutoOrganize = value == this.Parent.AutoOrganize ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(storage => storage.CarryChest);
        set => this.ActualOptions.CarryChest = value == this.Parent.CarryChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(storage => storage.CategorizeChest);
        set =>
            this.ActualOptions.CategorizeChest = value == this.Parent.CategorizeChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.Get(storage => storage.CategorizeChestBlockItems);
        set =>
            this.ActualOptions.CategorizeChestBlockItems =
                value == this.Parent.CategorizeChestBlockItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public string CategorizeChestSearchTerm
    {
        get =>
            string.IsNullOrWhiteSpace(this.ActualOptions.CategorizeChestSearchTerm)
                ? this.Parent.CategorizeChestSearchTerm
                : this.ActualOptions.CategorizeChestSearchTerm;
        set => this.ActualOptions.CategorizeChestSearchTerm = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.Get(storage => storage.CategorizeChestIncludeStacks);
        set =>
            this.ActualOptions.CategorizeChestIncludeStacks =
                value == this.Parent.CategorizeChestIncludeStacks ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(storage => storage.ChestFinder);
        set => this.ActualOptions.ChestFinder = value == this.Parent.ChestFinder ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.Get(storage => storage.ChestInfo);
        set => this.ActualOptions.ChestInfo = value == this.Parent.ChestInfo ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(storage => storage.CollectItems);
        set => this.ActualOptions.CollectItems = value == this.Parent.CollectItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(storage => storage.ConfigureChest);
        set => this.ActualOptions.ConfigureChest = value == this.Parent.ConfigureChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.Get(storage => storage.CookFromChest);
        set => this.ActualOptions.CookFromChest = value == this.Parent.CookFromChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(storage => storage.CraftFromChest);
        set => this.ActualOptions.CraftFromChest = value == this.Parent.CraftFromChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get =>
            this.ActualOptions.CraftFromChestDistance == 0
                ? this.Parent.CraftFromChestDistance
                : this.ActualOptions.CraftFromChestDistance;
        set => this.ActualOptions.CraftFromChestDistance = value == this.Parent.CraftFromChestDistance ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(storage => storage.HslColorPicker);
        set => this.ActualOptions.HslColorPicker = value == this.Parent.HslColorPicker ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(storage => storage.OpenHeldChest);
        set => this.ActualOptions.OpenHeldChest = value == this.Parent.OpenHeldChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(storage => storage.ResizeChest);
        set => this.ActualOptions.ResizeChest = value == this.Parent.ResizeChest ? ChestMenuOption.Default : value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get =>
            this.ActualOptions.ResizeChestCapacity == 0
                ? this.Parent.ResizeChestCapacity
                : this.ActualOptions.ResizeChestCapacity;
        set => this.ActualOptions.ResizeChestCapacity = value == this.Parent.ResizeChestCapacity ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(storage => storage.SearchItems);
        set => this.ActualOptions.SearchItems = value == this.Parent.SearchItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(storage => storage.ShopFromChest);
        set => this.ActualOptions.ShopFromChest = value == this.Parent.ShopFromChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(storage => storage.StashToChest);
        set => this.ActualOptions.StashToChest = value == this.Parent.StashToChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get =>
            this.ActualOptions.StashToChestDistance == 0
                ? this.Parent.StashToChestDistance
                : this.ActualOptions.StashToChestDistance;
        set => this.ActualOptions.StashToChestDistance = value == this.Parent.StashToChestDistance ? 0 : value;
    }

    /// <inheritdoc />
    public StashPriority StashToChestPriority
    {
        get =>
            this.ActualOptions.StashToChestPriority == 0
                ? this.Parent.StashToChestPriority
                : this.ActualOptions.StashToChestPriority;
        set => this.ActualOptions.StashToChestPriority = value == this.Parent.StashToChestPriority ? 0 : value;
    }

    /// <inheritdoc />
    public string StorageName
    {
        get => this.ActualOptions.StorageName;
        set => this.ActualOptions.StorageName = value;
    }

    /// <inheritdoc />
    public virtual string GetDescription() => this.Parent.GetDescription();

    /// <inheritdoc />
    public IStorageOptions GetActualOptions() => this.child.GetActualOptions();

    /// <inheritdoc />
    public IStorageOptions GetParentOptions() => this.Parent;

    /// <inheritdoc />
    public virtual string GetDisplayName() => this.Parent.GetDisplayName();

    private ChestMenuOption Get(Func<IStorageOptions, ChestMenuOption> selector)
    {
        var childValue = selector(this.ActualOptions);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue == ChestMenuOption.Disabled => ChestMenuOption.Disabled,
            ChestMenuOption.Default => parentValue,
            _ => childValue,
        };
    }

    private FeatureOption Get(Func<IStorageOptions, FeatureOption> selector)
    {
        var childValue = selector(this.ActualOptions);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => parentValue,
            _ => childValue,
        };
    }

    private RangeOption Get(Func<IStorageOptions, RangeOption> selector)
    {
        var childValue = selector(this.ActualOptions);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue == RangeOption.Disabled => RangeOption.Disabled,
            RangeOption.Default => parentValue,
            _ => childValue,
        };
    }
}