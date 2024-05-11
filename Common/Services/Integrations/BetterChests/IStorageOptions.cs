namespace StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Configurable options for a storage container.</summary>
public interface IStorageOptions
{
    /// <summary>Gets the name of the container.</summary>
    string DisplayName { get; }

    /// <summary>Gets the description of the container.</summary>
    string Description { get; }

    /// <summary>Gets or sets a value indicate if chests can be remotely accessed.</summary>
    public RangeOption AccessChest { get; set; }

    /// <summary>Gets or sets a value indicating the priority that chests will be accessed.</summary>
    public int AccessChestPriority { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be automatically organized overnight.</summary>
    public FeatureOption AutoOrganize { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be carried by the player.</summary>
    public FeatureOption CarryChest { get; set; }

    /// <summary>Gets or sets a value indicating if can have categories added to it, and which items can be added.</summary>
    public FeatureOption CategorizeChest { get; set; }

    /// <summary>Gets or sets a value indicating whether uncategorized items will be blocked.</summary>
    public FeatureOption CategorizeChestBlockItems { get; set; }

    /// <summary>Gets or sets the search term for categorizing items in the chest.</summary>
    public string CategorizeChestSearchTerm { get; set; }

    /// <summary>Gets or sets a value indicating whether categorization includes existing stacks by default.</summary>
    public FeatureOption CategorizeChestIncludeStacks { get; set; }

    /// <summary>Gets or sets a value indicating whether chests  in the current location can be searched for.</summary>
    public FeatureOption ChestFinder { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can collect dropped items.</summary>
    public FeatureOption CollectItems { get; set; }

    /// <summary>Gets or sets a value indicating whether chests can be configured.</summary>
    public FeatureOption ConfigureChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely cooked from.</summary>
    public RangeOption CookFromChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely crafted from.</summary>
    public RangeOption CraftFromChest { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely crafted from.</summary>
    public int CraftFromChestDistance { get; set; }

    /// <summary>Gets or sets a value indicating if the color picker will be replaced by an hsl color picker.</summary>
    public FeatureOption HslColorPicker { get; set; }

    /// <summary>Gets or sets a value indicating if inventory tabs will be added to the chest menu.</summary>
    public FeatureOption InventoryTabs { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be opened while it's being carried in the players inventory.</summary>
    public FeatureOption OpenHeldChest { get; set; }

    /// <summary>Gets or sets the menu for the chest.</summary>
    public ChestMenuOption ResizeChest { get; set; }

    /// <summary>Gets or sets the chest's carrying capacity.</summary>
    public int ResizeChestCapacity { get; set; }

    /// <summary>Gets or sets a value indicating if a search bar will be added to the chest menu.</summary>
    public FeatureOption SearchItems { get; set; }

    /// <summary>Gets or sets a value indicating if the shops can use items from the chest.</summary>
    public FeatureOption ShopFromChest { get; set; }

    /// <summary>Gets or sets a value indicating if storage can be sorted using a custom key.</summary>
    public FeatureOption SortInventory { get; set; }

    /// <summary>Gets or sets what the storage will be sorted by.</summary>
    public string SortInventoryBy { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely stashed into.</summary>
    public RangeOption StashToChest { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely stashed into.</summary>
    public int StashToChestDistance { get; set; }

    /// <summary>Gets or sets a value indicating the priority that chests will be stashed into.</summary>
    public StashPriority StashToChestPriority { get; set; }

    /// <summary>Gets or sets an icon to use for the storage.</summary>
    public string StorageIcon { get; set; }

    /// <summary>Gets or sets a value indicating whether info will be displayed about the chest.</summary>
    public FeatureOption StorageInfo { get; set; }

    /// <summary>Gets or sets a value indicating whether info will be displayed on hovering over a storage.</summary>
    public FeatureOption StorageInfoHover { get; set; }

    /// <summary>Gets or sets the name of the chest.</summary>
    public string StorageName { get; set; }
}