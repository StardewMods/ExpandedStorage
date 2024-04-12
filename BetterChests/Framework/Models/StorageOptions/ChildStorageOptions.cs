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

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(storage => storage.AutoOrganize);
        set => this.child.AutoOrganize = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(storage => storage.CarryChest);
        set => this.child.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(storage => storage.ChestFinder);
        set => this.child.ChestFinder = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.Get(storage => storage.ChestInfo);
        set => this.child.ChestInfo = value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(storage => storage.CollectItems);
        set => this.child.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(storage => storage.ConfigureChest);
        set => this.child.ConfigureChest = value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(storage => storage.CraftFromChest);
        set => this.child.CraftFromChest = value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get =>
            this.child.CraftFromChestDistance == 0
                ? this.getParent().CraftFromChestDistance
                : this.child.CraftFromChestDistance;
        set => this.child.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(storage => storage.HslColorPicker);
        set => this.child.HslColorPicker = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(storage => storage.CategorizeChest);
        set => this.child.CategorizeChest = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestAutomatically
    {
        get => this.Get(storage => storage.CategorizeChestAutomatically);
        set => this.child.CategorizeChestAutomatically = value;
    }

    /// <inheritdoc />
    public FilterMethod CategorizeChestMethod
    {
        get => this.child.CategorizeChestMethod;
        set => this.child.CategorizeChestMethod = value;
    }

    /// <inheritdoc />
    public HashSet<string> CategorizeChestTags
    {
        get => this.child.CategorizeChestTags.Union(this.getParent().CategorizeChestTags).ToHashSet();
        set => this.child.CategorizeChestTags = value;
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(storage => storage.InventoryTabs);
        set => this.child.InventoryTabs = value;
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.child.InventoryTabList.Union(this.getParent().InventoryTabList).ToHashSet();
        set => this.child.InventoryTabList = value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(storage => storage.OpenHeldChest);
        set => this.child.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(storage => storage.ResizeChest);
        set => this.child.ResizeChest = value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.child.ResizeChestCapacity;
        set => this.child.ResizeChestCapacity = value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(storage => storage.SearchItems);
        set => this.child.SearchItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(storage => storage.ShopFromChest);
        set => this.child.ShopFromChest = value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(storage => storage.StashToChest);
        set => this.child.StashToChest = value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get =>
            this.child.StashToChestDistance == 0
                ? this.getParent().StashToChestDistance
                : this.child.StashToChestDistance;
        set => this.child.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get =>
            this.child.StashToChestPriority == 0
                ? this.getParent().StashToChestPriority
                : this.child.StashToChestPriority;
        set => this.child.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public virtual string GetDescription() => this.getParent().GetDescription();

    /// <inheritdoc />
    public virtual string GetDisplayName() => this.getParent().GetDisplayName();

    private ChestMenuOption Get(Func<IStorageOptions, ChestMenuOption> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.getParent());
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
        var parentValue = selector(this.getParent());
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
        var parentValue = selector(this.getParent());
        return childValue switch
        {
            _ when parentValue == RangeOption.Disabled => RangeOption.Disabled,
            RangeOption.Default => parentValue,
            _ => childValue,
        };
    }
}