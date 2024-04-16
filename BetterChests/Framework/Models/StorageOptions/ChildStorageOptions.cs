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

    private IStorageOptions Parent => this.getParent();

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(storage => storage.AutoOrganize);
        set => this.child.AutoOrganize = value == this.Parent.AutoOrganize ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(storage => storage.CarryChest);
        set => this.child.CarryChest = value == this.Parent.CarryChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(storage => storage.ChestFinder);
        set => this.child.ChestFinder = value == this.Parent.ChestFinder ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.Get(storage => storage.ChestInfo);
        set => this.child.ChestInfo = value == this.Parent.ChestInfo ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(storage => storage.CollectItems);
        set => this.child.CollectItems = value == this.Parent.CollectItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(storage => storage.ConfigureChest);
        set => this.child.ConfigureChest = value == this.Parent.ConfigureChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(storage => storage.CraftFromChest);
        set => this.child.CraftFromChest = value == this.Parent.CraftFromChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get =>
            this.child.CraftFromChestDistance == 0
                ? this.Parent.CraftFromChestDistance
                : this.child.CraftFromChestDistance;
        set => this.child.CraftFromChestDistance = value == this.Parent.CraftFromChestDistance ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(storage => storage.HslColorPicker);
        set => this.child.HslColorPicker = value == this.Parent.HslColorPicker ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(storage => storage.CategorizeChest);
        set => this.child.CategorizeChest = value == this.Parent.CategorizeChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestAutomatically
    {
        get => this.Get(storage => storage.CategorizeChestAutomatically);
        set =>
            this.child.CategorizeChestAutomatically =
                value == this.Parent.CategorizeChestAutomatically ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FilterMethod CategorizeChestMethod
    {
        get => this.child.CategorizeChestMethod;
        set =>
            this.child.CategorizeChestMethod =
                value == this.Parent.CategorizeChestMethod ? FilterMethod.Default : value;
    }

    /// <inheritdoc />
    public HashSet<string> CategorizeChestTags
    {
        get => this.child.CategorizeChestTags.Union(this.Parent.CategorizeChestTags).ToHashSet();
        set => this.child.CategorizeChestTags = value.Except(this.Parent.CategorizeChestTags).ToHashSet();
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(storage => storage.InventoryTabs);
        set => this.child.InventoryTabs = value == this.Parent.InventoryTabs ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.child.InventoryTabList.Union(this.Parent.InventoryTabList).ToHashSet();
        set => this.child.InventoryTabList = value.Except(this.Parent.InventoryTabList).ToHashSet();
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(storage => storage.OpenHeldChest);
        set => this.child.OpenHeldChest = value == this.Parent.OpenHeldChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(storage => storage.ResizeChest);
        set => this.child.ResizeChest = value == this.Parent.ResizeChest ? ChestMenuOption.Default : value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.child.ResizeChestCapacity == 0 ? this.Parent.ResizeChestCapacity : this.child.ResizeChestCapacity;
        set => this.child.ResizeChestCapacity = value == this.Parent.ResizeChestCapacity ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(storage => storage.SearchItems);
        set => this.child.SearchItems = value == this.Parent.SearchItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(storage => storage.ShopFromChest);
        set => this.child.ShopFromChest = value == this.Parent.ShopFromChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(storage => storage.StashToChest);
        set => this.child.StashToChest = value == this.Parent.StashToChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get =>
            this.child.StashToChestDistance == 0 ? this.Parent.StashToChestDistance : this.child.StashToChestDistance;
        set => this.child.StashToChestDistance = value == this.Parent.StashToChestDistance ? 0 : value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get =>
            this.child.StashToChestPriority == 0 ? this.Parent.StashToChestPriority : this.child.StashToChestPriority;
        set => this.child.StashToChestPriority = value == this.Parent.StashToChestPriority ? 0 : value;
    }

    /// <inheritdoc />
    public virtual string GetDescription() => this.Parent.GetDescription();

    /// <inheritdoc />
    public virtual string GetDisplayName() => this.Parent.GetDisplayName();

    /// <summary>Reload the backing data.</summary>
    public virtual void RefreshData() { }

    private ChestMenuOption Get(Func<IStorageOptions, ChestMenuOption> selector)
    {
        var childValue = selector(this.child);
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
        var childValue = selector(this.child);
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
        var childValue = selector(this.child);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue == RangeOption.Disabled => RangeOption.Disabled,
            RangeOption.Default => parentValue,
            _ => childValue,
        };
    }
}