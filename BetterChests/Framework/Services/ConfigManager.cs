namespace StardewMods.BetterChests.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly ILog log;
    private readonly IManifest manifest;
    private readonly IModRegistry modRegistry;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ConfigManager(
        IDataHelper dataHelper,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        ILog log,
        IManifest manifest,
        IModHelper modHelper,
        IModRegistry modRegistry)
        : base(dataHelper, eventManager, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.log = log;
        this.manifest = manifest;
        this.modRegistry = modRegistry;

        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <inheritdoc />
    public bool AccessChestsShowArrows => this.Config.AccessChestsShowArrows;

    /// <inheritdoc />
    public int CarryChestLimit => this.Config.CarryChestLimit;

    /// <inheritdoc />
    public float CarryChestSlowAmount => this.Config.CarryChestSlowAmount;

    /// <inheritdoc />
    public int CarryChestSlowLimit => this.Config.CarryChestSlowLimit;

    /// <inheritdoc />
    public Controls Controls => this.Config.Controls;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations => this.Config.CraftFromChestDisableLocations;

    /// <inheritdoc />
    public bool DebugMode => this.Config.DebugMode;

    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions => this.Config.DefaultOptions;

    /// <inheritdoc />
    public int HslColorPickerHueSteps => this.Config.HslColorPickerHueSteps;

    /// <inheritdoc />
    public int HslColorPickerLightnessSteps => this.Config.HslColorPickerLightnessSteps;

    /// <inheritdoc />
    public InventoryMenu.BorderSide HslColorPickerPlacement => this.Config.HslColorPickerPlacement;

    /// <inheritdoc />
    public int HslColorPickerSaturationSteps => this.Config.HslColorPickerSaturationSteps;

    /// <inheritdoc />
    public List<TabData> InventoryTabList => this.Config.InventoryTabList;

    /// <inheritdoc />
    public FeatureOption LockItem => this.Config.LockItem;

    /// <inheritdoc />
    public bool LockItemHold => this.Config.LockItemHold;

    /// <inheritdoc />
    public FilterMethod SearchItemsMethod => this.Config.SearchItemsMethod;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations => this.Config.StashToChestDisableLocations;

    /// <inheritdoc />
    public HashSet<StorageInfoItem> StorageInfoHoverItems => this.Config.StorageInfoHoverItems;

    /// <inheritdoc />
    public HashSet<StorageInfoItem> StorageInfoMenuItems => this.Config.StorageInfoMenuItems;

    /// <inheritdoc />
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions => this.Config.StorageOptions;

    /// <summary>Adds the main options to the config menu.</summary>
    /// <param name="modManifest">The manifest.</param>
    /// <param name="id">The page id.</param>
    /// <param name="getTitle">Gets the title for the page.</param>
    /// <param name="options">The storage options to add.</param>
    /// <param name="isDefault">Indicates if these are the default options being set.</param>
    /// <param name="parentOptions">The options that this inherits from.</param>
    public void AddMainOption(
        IManifest modManifest,
        string? id,
        Func<string>? getTitle,
        IStorageOptions options,
        bool isDefault = false,
        IStorageOptions? parentOptions = null)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        if (!string.IsNullOrWhiteSpace(id) && getTitle is not null)
        {
            gmcm.AddPage(modManifest, id, getTitle);
        }

        if (parentOptions is not null)
        {
            gmcm.AddParagraph(
                modManifest,
                () => $"{I18n.Config_DefaultOption_Description(I18n.Config_DefaultOption_Indicator())}");
        }

        var featureOptions = (isDefault && parentOptions is null
            ? FeatureOptionExtensions.GetNames().Except(new[] { FeatureOption.Default.ToStringFast() })
            : FeatureOptionExtensions.GetNames()).ToArray();

        var rangeOptions = (isDefault && parentOptions is null
            ? RangeOptionExtensions.GetNames().Except(new[] { RangeOption.Default.ToStringFast() })
            : RangeOptionExtensions.GetNames()).ToArray();

        // Access Chest
        if (isDefault || this.DefaultOptions.AccessChest != RangeOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.AccessChest.ToStringFast(),
                value => options.AccessChest = RangeOptionExtensions.TryParse(value, out var range)
                    ? range
                    : RangeOption.Default,
                I18n.Config_AccessChests_Name,
                I18n.Config_AccessChests_Tooltip,
                rangeOptions,
                Localized.FormatRange(parentOptions?.AccessChest));
        }

        // Auto Organize
        if (isDefault || this.DefaultOptions.AutoOrganize != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.AutoOrganize.ToStringFast(),
                value => options.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.AutoOrganize));
        }

        // Carry Chest
        if (isDefault || this.DefaultOptions.CarryChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.CarryChest.ToStringFast(),
                value => options.CarryChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.CarryChest));
        }

        // Categorize Chest
        if (isDefault || this.DefaultOptions.CategorizeChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.CategorizeChest.ToStringFast(),
                value => options.CategorizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CategorizeChest_Name,
                I18n.Config_CategorizeChest_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.CategorizeChest));
        }

        // Chest Finder
        if (isDefault || this.DefaultOptions.ChestFinder != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.ChestFinder.ToStringFast(),
                value => options.ChestFinder = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestFinder_Name,
                I18n.Config_ChestFinder_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.ChestFinder));
        }

        // Collect Items
        if (isDefault || this.DefaultOptions.CollectItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.CollectItems.ToStringFast(),
                value => options.CollectItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.CollectItems));
        }

        // Configure Chest
        if (isDefault || this.DefaultOptions.ConfigureChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.ConfigureChest.ToStringFast(),
                value => options.ConfigureChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ConfigureChest_Name,
                I18n.Config_ConfigureChest_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.ConfigureChest));
        }

        // Craft from Chest
        if (isDefault || this.DefaultOptions.CookFromChest != RangeOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.CookFromChest.ToStringFast(),
                value => options.CookFromChest = RangeOptionExtensions.TryParse(value, out var range)
                    ? range
                    : RangeOption.Default,
                I18n.Config_CookFromChest_Name,
                I18n.Config_CookFromChest_Tooltip,
                rangeOptions,
                Localized.FormatRange(parentOptions?.CookFromChest));
        }

        // Craft from Chest
        if (isDefault || this.DefaultOptions.CraftFromChest != RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                modManifest,
                () => options.CraftFromChestDistance switch
                {
                    _ when options.CraftFromChest is RangeOption.Default => (int)RangeOption.Default,
                    _ when options.CraftFromChest is RangeOption.Disabled => (int)RangeOption.Disabled,
                    _ when options.CraftFromChest is RangeOption.Inventory => (int)RangeOption.Inventory,
                    _ when options.CraftFromChest is RangeOption.World => (int)RangeOption.World,
                    >= 2 when options.CraftFromChest is RangeOption.Location => (int)RangeOption.Location
                        + (int)Math.Ceiling(Math.Log2(options.CraftFromChestDistance))
                        - 1,
                    _ when options.CraftFromChest is RangeOption.Location => (int)RangeOption.World - 1,
                    _ => (int)RangeOption.Default,
                },
                value =>
                {
                    options.CraftFromChestDistance = value switch
                    {
                        (int)RangeOption.Default => 0,
                        (int)RangeOption.Disabled => 0,
                        (int)RangeOption.Inventory => 0,
                        (int)RangeOption.World => 0,
                        (int)RangeOption.World - 1 => -1,
                        >= (int)RangeOption.Location => (int)Math.Pow(2, 1 + value - (int)RangeOption.Location),
                        _ => 0,
                    };

                    options.CraftFromChest = value switch
                    {
                        (int)RangeOption.Default => RangeOption.Default,
                        (int)RangeOption.Disabled => RangeOption.Disabled,
                        (int)RangeOption.Inventory => RangeOption.Inventory,
                        (int)RangeOption.World => RangeOption.World,
                        (int)RangeOption.World - 1 => RangeOption.Location,
                        _ => RangeOption.Location,
                    };
                },
                I18n.Config_CraftFromChest_Name,
                I18n.Config_CraftFromChest_Tooltip,
                isDefault ? (int)RangeOption.Disabled : (int)RangeOption.Default,
                (int)RangeOption.World,
                1,
                Localized.FormatDistance(parentOptions?.CraftFromChest, parentOptions?.CraftFromChestDistance ?? 0));
        }

        // HSL Color Picker
        if (isDefault || this.DefaultOptions.HslColorPicker != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.HslColorPicker.ToStringFast(),
                value => options.HslColorPicker = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_HslColorPicker_Name,
                I18n.Config_HslColorPicker_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.HslColorPicker));
        }

        // Inventory Tabs
        if (isDefault || this.DefaultOptions.InventoryTabs != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.InventoryTabs.ToStringFast(),
                value => options.InventoryTabs = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_InventoryTabs_Name,
                I18n.Config_InventoryTabs_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.InventoryTabs));
        }

        // Open Held Chest
        if (isDefault || this.DefaultOptions.OpenHeldChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.OpenHeldChest.ToStringFast(),
                value => options.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.OpenHeldChest));
        }

        // Resize Chest
        var size = (int)options.ResizeChest;
        gmcm.OnFieldChanged(
            modManifest,
            (fieldId, fieldValue) =>
            {
                if (fieldId == nameof(options.ResizeChest)
                    && fieldValue is string value
                    && ChestMenuOptionExtensions.TryParse(value, out var option))
                {
                    size = (int)option;
                }
            });

        gmcm.AddNumberOption(
            modManifest,
            () => options.ResizeChestCapacity switch
            {
                -1 => 5,
                _ when size > 1 => Math.Min(5, (int)Math.Ceiling((float)options.ResizeChestCapacity / size)),
                0 => 0,
                9 => 1,
                36 => 2,
                70 => 3,
                _ => Math.Min(5, (int)Math.Ceiling(options.ResizeChestCapacity / 70f)),
            },
            value => options.ResizeChestCapacity = value switch
            {
                5 => -1, 0 => 0, _ when size > 1 => value * size, 1 => 9, 2 => 36, 3 => 70, _ => 70 * value,
            },
            I18n.Config_ResizeChestCapacity_Name,
            I18n.Config_ResizeChestCapacity_Tooltip,
            0,
            5,
            1,
            Localized.FormatCapacity(parentOptions?.ResizeChestCapacity ?? 0, () => size));

        // Resize Chest Menu
        if (isDefault || this.DefaultOptions.ResizeChest != ChestMenuOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.ResizeChest.ToStringFast(),
                value => options.ResizeChest = ChestMenuOptionExtensions.TryParse(value, out var option)
                    ? option
                    : ChestMenuOption.Default,
                I18n.Config_ResizeChestMenu_Name,
                I18n.Config_ResizeChestMenu_Tooltip,
                ChestMenuOptionExtensions.GetNames(),
                Localized.FormatMenuSize(parentOptions?.ResizeChest),
                nameof(options.ResizeChest));
        }

        // Search Items
        if (isDefault || this.DefaultOptions.SearchItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.SearchItems.ToStringFast(),
                value => options.SearchItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.SearchItems));
        }

        // Shop from Chest
        if (isDefault || this.DefaultOptions.ShopFromChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.ShopFromChest.ToStringFast(),
                value => options.ShopFromChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ShopFromChest_Name,
                I18n.Config_ShopFromChest_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.ShopFromChest));
        }

        // Sort Inventory
        if (isDefault || this.DefaultOptions.SortInventory != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.SortInventory.ToStringFast(),
                value => options.SortInventory = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SortInventory_Name,
                I18n.Config_SortInventory_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.SortInventory));

            gmcm.AddTextOption(
                modManifest,
                () => options.SortInventoryBy,
                value => options.SortInventoryBy = value,
                I18n.Config_SortInventoryBy_Name,
                I18n.Config_SortInventoryBy_Tooltip);
        }

        // Stash to Chest
        if (isDefault || this.DefaultOptions.StashToChest != RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                modManifest,
                () => options.StashToChestDistance switch
                {
                    _ when options.StashToChest is RangeOption.Default => (int)RangeOption.Default,
                    _ when options.StashToChest is RangeOption.Disabled => (int)RangeOption.Disabled,
                    _ when options.StashToChest is RangeOption.Inventory => (int)RangeOption.Inventory,
                    _ when options.StashToChest is RangeOption.World => (int)RangeOption.World,
                    >= 2 when options.StashToChest is RangeOption.Location => (int)RangeOption.Location
                        + (int)Math.Ceiling(Math.Log2(options.StashToChestDistance))
                        - 1,
                    _ when options.StashToChest is RangeOption.Location => (int)RangeOption.World - 1,
                    _ => (int)RangeOption.Default,
                },
                value =>
                {
                    options.StashToChestDistance = value switch
                    {
                        (int)RangeOption.Default => 0,
                        (int)RangeOption.Disabled => 0,
                        (int)RangeOption.Inventory => 0,
                        (int)RangeOption.World => 0,
                        (int)RangeOption.World - 1 => -1,
                        >= (int)RangeOption.Location => (int)Math.Pow(2, 1 + value - (int)RangeOption.Location),
                        _ => 0,
                    };

                    options.StashToChest = value switch
                    {
                        (int)RangeOption.Default => RangeOption.Default,
                        (int)RangeOption.Disabled => RangeOption.Disabled,
                        (int)RangeOption.Inventory => RangeOption.Inventory,
                        (int)RangeOption.World => RangeOption.World,
                        (int)RangeOption.World - 1 => RangeOption.Location,
                        _ => RangeOption.Location,
                    };
                },
                I18n.Config_StashToChest_Name,
                I18n.Config_StashToChest_Tooltip,
                isDefault ? (int)RangeOption.Disabled : (int)RangeOption.Default,
                (int)RangeOption.World,
                1,
                Localized.FormatDistance(parentOptions?.StashToChest, parentOptions?.StashToChestDistance ?? 0));
        }

        // Storage Info
        if (isDefault || this.DefaultOptions.StorageInfo != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                modManifest,
                () => options.StorageInfo.ToStringFast(),
                value => options.StorageInfo = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_StorageInfo_Name,
                I18n.Config_StorageInfo_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.StorageInfo));

            gmcm.AddTextOption(
                modManifest,
                () => options.StorageInfoHover.ToStringFast(),
                value => options.StorageInfoHover = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_StorageInfoHover_Name,
                I18n.Config_StorageInfoHover_Tooltip,
                featureOptions,
                Localized.FormatOption(parentOptions?.StorageInfoHover));
        }
    }

    /// <summary>Setup the main config options.</summary>
    public void SetupMainConfig()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();
        this.InitializeStorageTypes(config);

        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        gmcm.AddPageLink(this.manifest, "Main", I18n.Section_Main_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Main_Description);

        gmcm.AddPageLink(this.manifest, "Controls", I18n.Section_Controls_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Controls_Description);

        gmcm.AddPageLink(this.manifest, "Tweaks", I18n.Section_Tweaks_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Tweaks_Description);

        gmcm.AddSectionTitle(this.manifest, I18n.Section_Storages_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Storages_Description);

        gmcm.AddPageLink(this.manifest, "BigCraftables", I18n.Section_BigCraftables_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_BigCraftables_Description);

        gmcm.AddPageLink(this.manifest, "Furniture", I18n.Section_Furniture_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Furniture_Description);

        gmcm.AddPageLink(this.manifest, "Other", I18n.Section_Other_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Other_Description);

        var pages = new List<(string Id, string Title, string Description, IStorageOptions Options)>();
        var furnitureData = ItemRegistry.RequireTypeDefinition("(F)");
        foreach (var (dataType, storageTypes) in config.StorageOptions)
        {
            gmcm.AddPage(
                this.manifest,
                dataType switch { "BigCraftables" or "Furniture" => dataType, _ => "Other" },
                dataType switch
                {
                    "BigCraftables" => I18n.Section_BigCraftables_Name,
                    "Furniture" => I18n.Section_Furniture_Name,
                    _ => I18n.Section_Other_Name,
                });

            var subPages = new List<(string Id, string Title, string Description, IStorageOptions Options)>();
            foreach (var (storageId, storageOptions) in storageTypes)
            {
                string name;
                string description;

                switch (dataType)
                {
                    case "BigCraftables" when Game1.bigCraftableData.TryGetValue(storageId, out var bigCraftableData):
                        name = TokenParser.ParseText(bigCraftableData.DisplayName);
                        description = TokenParser.ParseText(bigCraftableData.Description);
                        break;
                    case "Buildings" when storageId == "Stable":
                        name = I18n.Storage_Saddlebag_Name();
                        description = I18n.Storage_Saddlebag_Tooltip();
                        break;
                    case "Buildings" when Game1.buildingData.TryGetValue(storageId, out var buildingData):
                        name = TokenParser.ParseText(buildingData.Name);
                        description = TokenParser.ParseText(buildingData.Description);
                        break;
                    case "Furniture":
                        var data = furnitureData.GetData(storageId);
                        name = TokenParser.ParseText(data.DisplayName);
                        description = TokenParser.ParseText(data.Description);
                        break;
                    case "Locations" when storageId == "FarmHouse":
                        name = I18n.Storage_Fridge_Name();
                        description = I18n.Storage_Fridge_Tooltip();
                        break;
                    case "Locations" when storageId == "IslandFarmHouse":
                        name = I18n.Storage_IslandFridge_Name();
                        description = I18n.Storage_IslandFridge_Tooltip();
                        break;
                    default: continue;
                }

                subPages.Add(($"{dataType}_{storageId}", name, description, storageOptions));
            }

            foreach (var (id, title, description, _) in subPages.OrderBy(page => page.Title))
            {
                gmcm.AddPageLink(this.manifest, id, () => title, () => description);
            }

            pages.AddRange(subPages);
        }

        this.AddMainOption(this.manifest, "Main", I18n.Section_Main_Name, config.DefaultOptions, true);

        gmcm.AddPage(this.manifest, "Controls", I18n.Section_Controls_Name);
        this.AddControls(config.Controls);

        gmcm.AddPage(this.manifest, "Tweaks", I18n.Section_Tweaks_Name);
        this.AddTweaks(config);

        foreach (var (id, title, _, options) in pages)
        {
            this.AddMainOption(this.manifest, id, () => title, options, true, config.DefaultOptions);
        }
    }

    private void AddControls(Controls controls)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

        // Access Chests
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.AccessChests,
            value => controls.AccessChests = value,
            I18n.Controls_AccessChests_Name,
            I18n.Controls_AccessChests_Tooltip);

        // Access Previous Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.AccessPreviousChest,
            value => controls.AccessPreviousChest = value,
            I18n.Controls_AccessPreviousChest_Name,
            I18n.Controls_AccessPreviousChest_Tooltip);

        // Access Next Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.AccessNextChest,
            value => controls.AccessNextChest = value,
            I18n.Controls_AccessNextChest_Name,
            I18n.Controls_AccessNextChest_Tooltip);

        // Clear Search
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ClearSearch,
            value => controls.ClearSearch = value,
            I18n.Controls_ClearSearch_Name,
            I18n.Controls_ClearSearch_Tooltip);

        // Configure Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ConfigureChest,
            value => controls.ConfigureChest = value,
            I18n.Controls_ConfigureChest_Name,
            I18n.Controls_ConfigureChest_Tooltip);

        // Chest Finder
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.OpenFoundChest,
            value => controls.OpenFoundChest = value,
            I18n.Controls_OpenFoundChest_Name,
            I18n.Controls_OpenFoundChest_Tooltip);

        // Craft from Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.OpenCrafting,
            value => controls.OpenCrafting = value,
            I18n.Controls_OpenCrafting_Name,
            I18n.Controls_OpenCrafting_Tooltip);

        // Stash to Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.StashItems,
            value => controls.StashItems = value,
            I18n.Controls_StashItems_Name,
            I18n.Controls_StashItems_Tooltip);

        // Resize Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ScrollUp,
            value => controls.ScrollUp = value,
            I18n.Controls_ScrollUp_Name,
            I18n.Controls_ScrollUp_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ScrollDown,
            value => controls.ScrollDown = value,
            I18n.Controls_ScrollDown_Name,
            I18n.Controls_ScrollDown_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ScrollPage,
            value => controls.ScrollPage = value,
            I18n.Controls_ScrollPage_Name,
            I18n.Controls_ScrollPage_Tooltip);

        // Lock Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.LockSlot,
            value => controls.LockSlot = value,
            I18n.Controls_LockItem_Name,
            I18n.Controls_LockItem_Tooltip);

        // Chest Info
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ToggleInfo,
            value => controls.ToggleInfo = value,
            I18n.Controls_ToggleInfo_Name,
            I18n.Controls_ToggleInfo_Tooltip);

        // Collect Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ToggleCollectItems,
            value => controls.ToggleCollectItems = value,
            I18n.Controls_ToggleCollectItems_Name,
            I18n.Controls_ToggleCollectItems_Tooltip);

        // Search Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ToggleSearch,
            value => controls.ToggleSearch = value,
            I18n.Controls_ToggleSearch_Name,
            I18n.Controls_ToggleSearch_Tooltip);

        // Transfer Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.TransferItems,
            value => controls.TransferItems = value,
            I18n.Controls_TransferItems_Name,
            I18n.Controls_TransferItems_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.TransferItemsReverse,
            value => controls.TransferItemsReverse = value,
            I18n.Controls_TransferItemsReverse_Name,
            I18n.Controls_TransferItemsReverse_Tooltip);

        // Copy
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.Copy,
            value => controls.Copy = value,
            I18n.Controls_Copy_Name,
            I18n.Controls_Copy_Tooltip);

        // Paste
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.Paste,
            value => controls.Paste = value,
            I18n.Controls_Paste_Name,
            I18n.Controls_Paste_Tooltip);
    }

    private void AddTweaks(DefaultConfig config)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

        // Access Chest Show Arrows
        gmcm.AddBoolOption(
            this.manifest,
            () => config.AccessChestsShowArrows,
            value => config.AccessChestsShowArrows = value,
            I18n.Config_AccessChestsShowArrows_Name,
            I18n.Config_AccessChestsShowArrows_Tooltip);

        // Carry Chest Limit
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CarryChestLimit,
            value => config.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip,
            0,
            36,
            1,
            Localized.CarryChestLimit);

        gmcm.AddNumberOption(
            this.manifest,
            () => config.CarryChestSlowAmount,
            value => config.CarryChestSlowAmount = value,
            I18n.Config_CarryChestSlowAmount_Name,
            I18n.Config_CarryChestSlowAmount_Tooltip);

        // Carry Chest Slow Limit
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CarryChestSlowLimit,
            value => config.CarryChestSlowLimit = value,
            I18n.Config_CarryChestSlowLimit_Name,
            I18n.Config_CarryChestSlowLimit_Tooltip,
            0,
            4,
            1,
            Localized.CarryChestLimit);

        // Hsl Color Picker Steps
        gmcm.AddNumberOption(
            this.manifest,
            () => config.HslColorPickerHueSteps,
            value => config.HslColorPickerHueSteps = value,
            I18n.Config_HslColorPickerHueSteps_Name,
            I18n.Config_HslColorPickerHueSteps_Tooltip,
            1,
            29,
            1);

        gmcm.AddNumberOption(
            this.manifest,
            () => config.HslColorPickerSaturationSteps,
            value => config.HslColorPickerSaturationSteps = value,
            I18n.Config_HslColorPickerSaturationSteps_Name,
            I18n.Config_HslColorPickerSaturationSteps_Tooltip,
            1,
            29,
            1);

        gmcm.AddNumberOption(
            this.manifest,
            () => config.HslColorPickerLightnessSteps,
            value => config.HslColorPickerLightnessSteps = value,
            I18n.Config_HslColorPickerLightnessSteps_Name,
            I18n.Config_HslColorPickerSaturationSteps_Tooltip,
            1,
            29,
            1);

        // Hsl Color Picker Placement
        const int left = (int)InventoryMenu.BorderSide.Left;
        const int right = (int)InventoryMenu.BorderSide.Right;
        var low = Math.Min(left, right);
        var high = Math.Max(left, right);

        gmcm.AddNumberOption(
            this.manifest,
            () => (int)config.HslColorPickerPlacement,
            value => config.HslColorPickerPlacement = (InventoryMenu.BorderSide)value,
            I18n.Config_HslColorPickerPlacement_Name,
            I18n.Config_HslColorPickerPlacement_Tooltip,
            low,
            high,
            high - low,
            Localized.Border);

        // Lock Item
        gmcm.AddTextOption(
            this.manifest,
            () => config.LockItem.ToStringFast(),
            value => config.LockItem =
                FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_LockItem_Name,
            I18n.Config_LockItem_Tooltip,
            FeatureOptionExtensions.GetNames(),
            Localized.FormatOption());

        // Lock Item Hold
        gmcm.AddBoolOption(
            this.manifest,
            () => config.LockItemHold,
            value => config.LockItemHold = value,
            I18n.Config_LockItemHold_Name,
            I18n.Config_LockItemHold_Tooltip);

        // Search Items Method
        gmcm.AddTextOption(
            this.manifest,
            () => config.SearchItemsMethod.ToStringFast(),
            value => config.SearchItemsMethod =
                FilterMethodExtensions.TryParse(value, out var method) ? method : FilterMethod.Default,
            I18n.Config_SearchItemsMethod_Name,
            I18n.Config_SearchItemsMethod_Tooltip,
            FilterMethodExtensions.GetNames(),
            Localized.FormatMethod());

        // Storage Info Hover Items
        gmcm.AddTextOption(
            this.manifest,
            () => string.Join(',', config.StorageInfoHoverItems.Select(item => item.ToStringFast())),
            value => config.StorageInfoHoverItems = [..GetStorageInfoItems(value)],
            I18n.Config_StorageInfoHoverItems_Name,
            I18n.Config_StorageInfoHoverItems_Tooltip);

        // Storage Info Menu Items
        gmcm.AddTextOption(
            this.manifest,
            () => string.Join(',', config.StorageInfoMenuItems.Select(item => item.ToStringFast())),
            value => config.StorageInfoMenuItems = [..GetStorageInfoItems(value)],
            I18n.Config_StorageInfoMenuItems_Name,
            I18n.Config_StorageInfoMenuItems_Tooltip);

        return;

        IEnumerable<StorageInfoItem> GetStorageInfoItems(string value)
        {
            var items = value.Split(',');
            foreach (var item in items)
            {
                if (StorageInfoItemExtensions.TryParse(item, out var infoItem))
                {
                    yield return infoItem;
                }
            }
        }
    }

    private void InitializeDefaultOptions(IStorageOptions options)
    {
        var defaultOptions = this.GetDefault().DefaultOptions;
        options.ForEachOption(
            (name, option) =>
            {
                switch (option)
                {
                    case FeatureOption.Default when defaultOptions.TryGetOption(name, out FeatureOption defaultOption):
                        options.SetOption(name, defaultOption);
                        return;

                    case RangeOption.Default when defaultOptions.TryGetOption(name, out RangeOption defaultOption):
                        options.SetOption(name, defaultOption);
                        return;

                    case ChestMenuOption.Default when defaultOptions.TryGetOption(
                        name,
                        out ChestMenuOption defaultOption):
                        options.SetOption(name, defaultOption);
                        return;

                    case string currentOption when string.IsNullOrWhiteSpace(currentOption)
                        && defaultOptions.TryGetOption(name, out string defaultOption):
                        options.SetOption(name, defaultOption);
                        return;

                    case 0 when defaultOptions.TryGetOption(name, out int defaultOption):
                        options.SetOption(name, defaultOption);
                        return;
                }
            });
    }

    private void InitializeStorageTypes(IModConfig config)
    {
        // Initialize Data Types
        config.StorageOptions.TryAdd("BigCraftables", []);
        config.StorageOptions.TryAdd("Buildings", []);
        config.StorageOptions.TryAdd("Furniture", []);
        config.StorageOptions.TryAdd("Locations", []);

        // Initialize Chest
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "130",
                new DefaultStorageOptions
                {
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = 70,
                });

        // Initialize Stone Chest
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "232",
                new DefaultStorageOptions
                {
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = 70,
                });

        // Initialize Junimo Chest
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "256",
                new DefaultStorageOptions
                {
                    CraftFromChest = RangeOption.World,
                    ResizeChest = ChestMenuOption.Medium,
                    ResizeChestCapacity = 36,
                    StashToChest = RangeOption.World,
                });

        // Initialize Mini-Fridge
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "216",
                new DefaultStorageOptions
                {
                    CookFromChest = RangeOption.Location,
                    HslColorPicker = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = -1,
                    SearchItems = FeatureOption.Disabled,
                });

        // Initialize Mini-Shipping Bin
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "248",
                new DefaultStorageOptions
                {
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Disabled,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize Big Chest
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "BigChest",
                new DefaultStorageOptions
                {
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = -1,
                });

        // Initialize Big Stone Chest
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "BigStoneChest",
                new DefaultStorageOptions
                {
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = -1,
                });

        // Initialize Auto-Grabber
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "165",
                new DefaultStorageOptions
                {
                    AccessChest = RangeOption.Disabled,
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Disabled,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize Workbench
        config
            .StorageOptions["BigCraftables"]
            .TryAdd(
                "208",
                new DefaultStorageOptions
                {
                    CraftFromChest = RangeOption.Location,
                    CraftFromChestDistance = -1,
                });

        // Initialize Junimo Hut
        config
            .StorageOptions["Buildings"]
            .TryAdd(
                "Junimo Hut",
                new DefaultStorageOptions
                {
                    AccessChest = RangeOption.Disabled,
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Disabled,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize Mill
        config
            .StorageOptions["Buildings"]
            .TryAdd(
                "Mill",
                new DefaultStorageOptions
                {
                    AccessChest = RangeOption.Disabled,
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CookFromChest = RangeOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Disabled,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize Shipping Bin
        config
            .StorageOptions["Buildings"]
            .TryAdd(
                "Shipping Bin",
                new DefaultStorageOptions
                {
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CookFromChest = RangeOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Medium,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize Horse Overhaul Saddlebags
        if (this.modRegistry.IsLoaded("Goldenrevolver.HorseOverhaul"))
        {
            config
                .StorageOptions["Buildings"]
                .TryAdd(
                    "Stable",
                    new DefaultStorageOptions
                    {
                        AutoOrganize = FeatureOption.Disabled,
                        CarryChest = FeatureOption.Disabled,
                        HslColorPicker = FeatureOption.Disabled,
                    });
        }

        // Initialize Dressers
        config.StorageOptions["Furniture"].TryAdd("704", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("709", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("714", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("719", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("JojaDresser", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("GrayJojaDresser", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("WizardDresser", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("JunimoDresser", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("RetroDresser", new DefaultStorageOptions());

        // Initialize Fish Tanks
        config.StorageOptions["Furniture"].TryAdd("2304", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("2312", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("2322", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("2400", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("2414", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("JungleTank", new DefaultStorageOptions());
        config.StorageOptions["Furniture"].TryAdd("CCFishTank", new DefaultStorageOptions());

        // Initialize FarmHouse
        config
            .StorageOptions["Locations"]
            .TryAdd(
                "FarmHouse",
                new DefaultStorageOptions
                {
                    CookFromChest = RangeOption.Location,
                    HslColorPicker = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = -1,
                    SearchItems = FeatureOption.Disabled,
                });

        // Initialize IslandFarmHouse
        config
            .StorageOptions["Locations"]
            .TryAdd(
                "IslandFarmHouse",
                new DefaultStorageOptions
                {
                    CookFromChest = RangeOption.Location,
                    HslColorPicker = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = -1,
                    SearchItems = FeatureOption.Disabled,
                });
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        this.InitializeDefaultOptions(this.Config.DefaultOptions);
        this.InitializeStorageTypes(this.Config);
        this.log.Trace("Config changed:\n{0}", e.Config);
    }

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupMainConfig();
        }
    }
}