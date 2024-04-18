namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal sealed class TemporaryStorageOptions : DefaultStorageOptions
{
    private readonly IStorageOptions defaultOptions;
    private readonly IStorageOptions storageOptions;

    /// <summary>Initializes a new instance of the <see cref="TemporaryStorageOptions" /> class.</summary>
    /// <param name="storageOptions">The storage options to copy.</param>
    /// <param name="defaultOptions">The default storage options.</param>
    public TemporaryStorageOptions(IStorageOptions storageOptions, IStorageOptions defaultOptions)
    {
        this.storageOptions = storageOptions;
        this.defaultOptions = defaultOptions;
        this.AccessChest = storageOptions.AccessChest;
        this.AutoOrganize = storageOptions.AutoOrganize;
        this.CarryChest = storageOptions.CarryChest;
        this.CategorizeChest = storageOptions.CategorizeChest;
        this.CategorizeChestBlockItems = storageOptions.CategorizeChestBlockItems;
        this.CategorizeChestSearchTerm = storageOptions.CategorizeChestSearchTerm;
        this.CategorizeChestIncludeStacks = storageOptions.CategorizeChestIncludeStacks;
        this.ChestFinder = storageOptions.ChestFinder;
        this.ChestInfo = storageOptions.ChestInfo;
        this.CollectItems = storageOptions.CollectItems;
        this.ConfigureChest = storageOptions.ConfigureChest;
        this.CookFromChest = storageOptions.CookFromChest;
        this.CraftFromChest = storageOptions.CraftFromChest;
        this.CraftFromChestDistance = storageOptions.CraftFromChestDistance;
        this.HslColorPicker = storageOptions.HslColorPicker;
        this.OpenHeldChest = storageOptions.OpenHeldChest;
        this.ResizeChest = storageOptions.ResizeChest;
        this.ResizeChestCapacity = storageOptions.ResizeChestCapacity;
        this.SearchItems = storageOptions.SearchItems;
        this.ShopFromChest = this.storageOptions.ShopFromChest;
        this.StashToChest = storageOptions.StashToChest;
        this.StashToChestDistance = storageOptions.StashToChestDistance;
        this.StashToChestPriority = storageOptions.StashToChestPriority;
        this.StorageName = storageOptions.StorageName;
    }

    /// <inheritdoc />
    public override string GetDisplayName() => this.storageOptions.GetDisplayName();

    /// <inheritdoc />
    public override string GetDescription() => this.storageOptions.GetDescription();

    /// <summary>Saves the options back to the default.</summary>
    public void Reset()
    {
        this.AccessChest = this.defaultOptions.AccessChest;
        this.AutoOrganize = this.defaultOptions.AutoOrganize;
        this.CarryChest = this.defaultOptions.CarryChest;
        this.CategorizeChest = this.defaultOptions.CategorizeChest;
        this.CategorizeChestBlockItems = this.defaultOptions.CategorizeChestBlockItems;
        this.CategorizeChestSearchTerm = this.defaultOptions.CategorizeChestSearchTerm;
        this.CategorizeChestIncludeStacks = this.defaultOptions.CategorizeChestIncludeStacks;
        this.ChestFinder = this.defaultOptions.ChestFinder;
        this.ChestInfo = this.defaultOptions.ChestInfo;
        this.CollectItems = this.defaultOptions.CollectItems;
        this.ConfigureChest = this.defaultOptions.ConfigureChest;
        this.CookFromChest = this.defaultOptions.CookFromChest;
        this.CraftFromChest = this.defaultOptions.CraftFromChest;
        this.CraftFromChestDistance = this.defaultOptions.CraftFromChestDistance;
        this.HslColorPicker = this.defaultOptions.HslColorPicker;
        this.OpenHeldChest = this.defaultOptions.OpenHeldChest;
        this.ResizeChest = this.defaultOptions.ResizeChest;
        this.ResizeChestCapacity = this.defaultOptions.ResizeChestCapacity;
        this.SearchItems = this.defaultOptions.SearchItems;
        this.ShopFromChest = this.defaultOptions.ShopFromChest;
        this.StashToChest = this.defaultOptions.StashToChest;
        this.StashToChestDistance = this.defaultOptions.StashToChestDistance;
        this.StashToChestPriority = this.defaultOptions.StashToChestPriority;
        this.StorageName = this.defaultOptions.StorageName;
    }

    /// <summary>Saves the changes back to storage options.</summary>
    public void Save()
    {
        this.storageOptions.AccessChest = this.AccessChest;
        this.storageOptions.AutoOrganize = this.AutoOrganize;
        this.storageOptions.CarryChest = this.CarryChest;
        this.storageOptions.CategorizeChest = this.CategorizeChest;
        this.storageOptions.CategorizeChestBlockItems = this.CategorizeChestBlockItems;
        this.storageOptions.CategorizeChestSearchTerm = this.CategorizeChestSearchTerm;
        this.storageOptions.CategorizeChestIncludeStacks = this.CategorizeChestIncludeStacks;
        this.storageOptions.ChestFinder = this.ChestFinder;
        this.storageOptions.ChestInfo = this.ChestInfo;
        this.storageOptions.CollectItems = this.CollectItems;
        this.storageOptions.ConfigureChest = this.ConfigureChest;
        this.storageOptions.CookFromChest = this.CookFromChest;
        this.storageOptions.CraftFromChest = this.CraftFromChest;
        this.storageOptions.CraftFromChestDistance = this.CraftFromChestDistance;
        this.storageOptions.HslColorPicker = this.HslColorPicker;
        this.storageOptions.OpenHeldChest = this.OpenHeldChest;
        this.storageOptions.ResizeChest = this.ResizeChest;
        this.storageOptions.ResizeChestCapacity = this.ResizeChestCapacity;
        this.storageOptions.SearchItems = this.SearchItems;
        this.storageOptions.ShopFromChest = this.ShopFromChest;
        this.storageOptions.StashToChest = this.StashToChest;
        this.storageOptions.StashToChestDistance = this.StashToChestDistance;
        this.storageOptions.StashToChestPriority = this.StashToChestPriority;
        this.storageOptions.StorageName = this.StorageName;
    }
}