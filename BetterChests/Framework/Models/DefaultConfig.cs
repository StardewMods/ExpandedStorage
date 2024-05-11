namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.Menus;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public bool AccessChestsShowArrows { get; set; } = true;

    /// <inheritdoc />
    public int CarryChestLimit { get; set; } = 3;

    /// <inheritdoc />
    public float CarryChestSlowAmount { get; set; } = -1f;

    /// <inheritdoc />
    public int CarryChestSlowLimit { get; set; } = 1;

    /// <inheritdoc />
    public Controls Controls { get; set; } = new();

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public bool DebugMode { get; set; }

    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions { get; set; } = new()
    {
        AccessChest = RangeOption.Location,
        AutoOrganize = FeatureOption.Enabled,
        CarryChest = FeatureOption.Enabled,
        CategorizeChest = FeatureOption.Enabled,
        CategorizeChestBlockItems = FeatureOption.Disabled,
        CategorizeChestIncludeStacks = FeatureOption.Enabled,
        ChestFinder = FeatureOption.Enabled,
        CollectItems = FeatureOption.Enabled,
        ConfigureChest = FeatureOption.Enabled,
        CookFromChest = RangeOption.Location,
        CraftFromChest = RangeOption.Location,
        CraftFromChestDistance = -1,
        HslColorPicker = FeatureOption.Enabled,
        InventoryTabs = FeatureOption.Enabled,
        OpenHeldChest = FeatureOption.Enabled,
        ResizeChest = ChestMenuOption.Large,
        ResizeChestCapacity = 70,
        SearchItems = FeatureOption.Enabled,
        ShopFromChest = FeatureOption.Enabled,
        SortInventory = FeatureOption.Enabled,
        StashToChest = RangeOption.Location,
        StashToChestDistance = 16,
        StorageInfo = FeatureOption.Enabled,
        StorageInfoHover = FeatureOption.Enabled,
    };

    /// <inheritdoc />
    public int HslColorPickerHueSteps { get; set; } = 29;

    /// <inheritdoc />
    public int HslColorPickerLightnessSteps { get; set; } = 16;

    /// <inheritdoc />
    public InventoryMenu.BorderSide HslColorPickerPlacement { get; set; } = InventoryMenu.BorderSide.Right;

    /// <inheritdoc />
    public int HslColorPickerSaturationSteps { get; set; } = 16;

    /// <inheritdoc />
    public List<TabData> InventoryTabList { get; set; } =
    [
        new TabData
        {
            Icon = "furyx639.BetterChests/Clothing",
            Label = I18n.Tabs_Clothing_Name(),
            SearchTerm = "category_clothing category_boots category_hat",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Cooking",
            Label = I18n.Tabs_Cooking_Name(),
            SearchTerm =
                "category_syrup category_artisan_goods category_ingredients category_sell_at_pierres_and_marnies category_sell_at_pierres category_meat category_cooking category_milk category_egg",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Crops",
            Label = I18n.Tabs_Crops_Name(),
            SearchTerm = "category_greens category_flowers category_fruits category_vegetable",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Equipment",
            Label = I18n.Tabs_Equipment_Name(),
            SearchTerm = "category_equipment category_ring category_tool category_weapon",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Fishing",
            Label = I18n.Tabs_Fishing_Name(),
            SearchTerm = "category_bait category_fish category_tackle category_sell_at_fish_shop",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Materials",
            Label = I18n.Tabs_Materials_Name(),
            SearchTerm =
                "category_monster_loot category_metal_resources category_building_resources category_minerals category_crafting category_gem",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Miscellaneous",
            Label = I18n.Tabs_Misc_Name(),
            SearchTerm = "category_big_craftable category_furniture category_junk",
        },
        new TabData
        {
            Icon = "furyx639.BetterChests/Seeds",
            Label = I18n.Tabs_Seeds_Name(),
            SearchTerm = "category_seeds category_fertilizer",
        },
    ];

    /// <inheritdoc />
    public FeatureOption LockItem { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public bool LockItemHold { get; set; } = true;

    /// <inheritdoc />
    public FilterMethod SearchItemsMethod { get; set; } = FilterMethod.GrayedOut;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public HashSet<StorageInfoItem> StorageInfoHoverItems { get; set; } =
    [
        StorageInfoItem.Icon,
        StorageInfoItem.Name,
        StorageInfoItem.Type,
        StorageInfoItem.Capacity,
        StorageInfoItem.TotalValue,
    ];

    /// <inheritdoc />
    public HashSet<StorageInfoItem> StorageInfoMenuItems { get; set; } =
    [
        StorageInfoItem.Type,
        StorageInfoItem.Location,
        StorageInfoItem.Position,
        StorageInfoItem.Inventory,
        StorageInfoItem.TotalItems,
        StorageInfoItem.UniqueItems,
        StorageInfoItem.TotalValue,
    ];

    /// <inheritdoc />
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions { get; set; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        this.ForEachConfig(
            (name, config) =>
            {
                switch (config)
                {
                    case DefaultStorageOptions defaultOptions:
                        sb.AppendLine("Default Options");
                        sb.AppendLine(defaultOptions.ToString());
                        break;

                    case Controls controls:
                        sb.AppendLine("Controls");
                        sb.AppendLine(controls.ToString());
                        break;

                    case Dictionary<string, Dictionary<string, DefaultStorageOptions>> storageOptions:
                        foreach (var (storageType, typeOptions) in storageOptions)
                        {
                            foreach (var (storageName, storageOption) in typeOptions)
                            {
                                sb.AppendLine(CultureInfo.InvariantCulture, $"{storageType}: {storageName}");
                                sb.AppendLine(storageOption.ToString());
                            }
                        }

                        break;

                    case FeatureOption featureOption:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {featureOption.ToStringFast()}");
                        break;

                    case FilterMethod filterMethod:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {filterMethod.ToStringFast()}");
                        break;

                    case InventoryMenu.BorderSide placement:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {placement.ToString()}");
                        break;

                    case HashSet<string> hashSet:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {string.Join(", ", hashSet)}");
                        break;

                    case HashSet<StorageInfoItem> hashSet:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {string.Join(", ", hashSet)}");
                        break;

                    case bool boolValue:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {boolValue}");
                        break;

                    case int intValue:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {intValue}");
                        break;

                    case float floatValue:
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{name}: {floatValue}");
                        break;
                }
            });

        return sb.ToString();
    }
}