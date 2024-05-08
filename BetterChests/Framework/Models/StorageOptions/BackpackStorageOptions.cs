namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <inheritdoc />
internal sealed class BackpackStorageOptions : IStorageOptions
{
    private readonly Farmer farmer;
    private readonly IStorageOptions storageOptions;

    /// <summary>Initializes a new instance of the <see cref="BackpackStorageOptions" /> class.</summary>
    /// <param name="farmer">The farmer whose backpack storage this represents.</param>
    public BackpackStorageOptions(Farmer farmer)
    {
        this.storageOptions = new ModDataStorageOptions(farmer.modData);
        this.farmer = farmer;
    }

    /// <inheritdoc />
    public string Description => I18n.Storage_Backpack_Tooltip();

    /// <inheritdoc />
    public string DisplayName => I18n.Storage_Backpack_Name();

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.storageOptions.AccessChest;
        set => this.storageOptions.AccessChest = value;
    }

    /// <inheritdoc />
    public int AccessChestPriority
    {
        get => this.storageOptions.AccessChestPriority;
        set => this.storageOptions.AccessChestPriority = value;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.storageOptions.AutoOrganize;
        set => this.storageOptions.AutoOrganize = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.storageOptions.CarryChest;
        set => this.storageOptions.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.storageOptions.CategorizeChest;
        set => this.storageOptions.CategorizeChest = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.storageOptions.CategorizeChestBlockItems;
        set => this.storageOptions.CategorizeChestBlockItems = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.storageOptions.CategorizeChestIncludeStacks;
        set => this.storageOptions.CategorizeChestIncludeStacks = value;
    }

    /// <inheritdoc />
    public string CategorizeChestSearchTerm
    {
        get => this.storageOptions.CategorizeChestSearchTerm;
        set => this.storageOptions.CategorizeChestSearchTerm = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.storageOptions.ChestFinder;
        set => this.storageOptions.ChestFinder = value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.storageOptions.CollectItems;
        set => this.storageOptions.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.storageOptions.ConfigureChest;
        set => this.storageOptions.ConfigureChest = value;
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.storageOptions.CookFromChest;
        set => this.storageOptions.CookFromChest = value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.storageOptions.CraftFromChest;
        set => this.storageOptions.CraftFromChest = value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.storageOptions.CraftFromChestDistance;
        set => this.storageOptions.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.storageOptions.HslColorPicker;
        set => this.storageOptions.HslColorPicker = value;
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.storageOptions.InventoryTabs;
        set => this.storageOptions.InventoryTabs = value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.storageOptions.OpenHeldChest;
        set => this.storageOptions.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => ChestMenuOption.Medium;
        set { }
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.farmer.MaxItems;
        set
        {
            this.farmer.MaxItems = value;
            this.farmer.Items.RemoveEmptySlots();
            while (this.farmer.Items.Count < this.farmer.MaxItems)
            {
                this.farmer.Items.Add(null);
            }
        }
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.storageOptions.SearchItems;
        set => this.storageOptions.SearchItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.storageOptions.ShopFromChest;
        set => this.storageOptions.ShopFromChest = value;
    }

    /// <inheritdoc />
    public FeatureOption SortInventory
    {
        get => this.storageOptions.SortInventory;
        set => this.storageOptions.SortInventory = value;
    }

    /// <inheritdoc />
    public string SortInventoryBy
    {
        get => this.storageOptions.SortInventoryBy;
        set => this.storageOptions.SortInventoryBy = value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.storageOptions.StashToChest;
        set => this.storageOptions.StashToChest = value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.storageOptions.StashToChestDistance;
        set => this.storageOptions.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public StashPriority StashToChestPriority
    {
        get => this.storageOptions.StashToChestPriority;
        set => this.storageOptions.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public string StorageIcon
    {
        get => this.storageOptions.StorageIcon;
        set => this.storageOptions.StorageIcon = value;
    }

    /// <inheritdoc />
    public FeatureOption StorageInfo
    {
        get => this.storageOptions.StorageInfo;
        set => this.storageOptions.StorageInfo = value;
    }

    /// <inheritdoc />
    public FeatureOption StorageInfoHover
    {
        get => this.storageOptions.StorageInfoHover;
        set => this.storageOptions.StorageInfoHover = value;
    }

    /// <inheritdoc />
    public string StorageName
    {
        get => this.farmer.Name;
        set { }
    }
}