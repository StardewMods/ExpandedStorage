namespace StardewMods.BetterChests.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.TokenizableStrings;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly LocalizedTextManager localizedTextManager;
    private readonly ILog log;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="localizedTextManager">Dependency used for formatting and translating text.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        LocalizedTextManager localizedTextManager,
        ILog log,
        IManifest manifest,
        IModHelper modHelper)
        : base(eventManager, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.localizedTextManager = localizedTextManager;
        this.log = log;
        this.manifest = manifest;

        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions => this.Config.DefaultOptions;

    /// <inheritdoc />
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions => this.Config.StorageOptions;

    /// <inheritdoc />
    public int CarryChestLimit => this.Config.CarryChestLimit;

    /// <inheritdoc />
    public int CarryChestSlowLimit => this.Config.CarryChestSlowLimit;

    /// <inheritdoc />
    public Controls Controls => this.Config.Controls;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations => this.Config.CraftFromChestDisableLocations;

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench => this.Config.CraftFromWorkbench;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance => this.Config.CraftFromWorkbenchDistance;

    /// <inheritdoc />
    public int HslColorPickerHueSteps => this.Config.HslColorPickerHueSteps;

    /// <inheritdoc />
    public int HslColorPickerSaturationSteps => this.Config.HslColorPickerSaturationSteps;

    /// <inheritdoc />
    public int HslColorPickerLightnessSteps => this.Config.HslColorPickerLightnessSteps;

    /// <inheritdoc />
    public FilterMethod InventoryTabMethod => this.Config.InventoryTabMethod;

    /// <inheritdoc />
    public FeatureOption LockItem => this.Config.LockItem;

    /// <inheritdoc />
    public bool LockItemHold => this.Config.LockItemHold;

    /// <inheritdoc />
    public FilterMethod SearchItemsMethod => this.Config.SearchItemsMethod;

    /// <inheritdoc />
    public char SearchTagSymbol => this.Config.SearchTagSymbol;

    /// <inheritdoc />
    public char SearchNegationSymbol => this.Config.SearchNegationSymbol;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations => this.Config.StashToChestDisableLocations;

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
        var pages = new List<(string Id, string Title, IStorageOptions Options)>();
        foreach (var (dataType, storageTypes) in config.StorageOptions)
        {
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
                    case "Buildings" when Game1.buildingData.TryGetValue(storageId, out var buildingData):
                        name = TokenParser.ParseText(buildingData.Name);
                        description = TokenParser.ParseText(buildingData.Description);
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

                pages.Add(($"{dataType}_{storageId}", name, storageOptions));
                gmcm.AddPageLink(this.manifest, $"{dataType}_{storageId}", () => name, () => description);
            }
        }

        gmcm.AddPage(this.manifest, "Main", I18n.Section_Main_Name);
        this.AddMainOption(config.DefaultOptions, true);

        gmcm.AddPage(this.manifest, "Controls", I18n.Section_Controls_Name);
        this.AddControls(config.Controls);

        gmcm.AddPage(this.manifest, "Tweaks", I18n.Section_Tweaks_Name);
        this.AddTweaks(config);

        foreach (var (id, title, options) in pages)
        {
            gmcm.AddPage(this.manifest, id, () => title);
            this.AddMainOption(options, true);
        }
    }

    /// <summary>Adds the main options to the config menu.</summary>
    /// <param name="options">The storage options to add.</param>
    /// <param name="isDefault">Indicates if these are the default options being set.</param>
    public void AddMainOption(IStorageOptions options, bool isDefault = false)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

        // Auto Organize
        if (isDefault || this.DefaultOptions.AutoOrganize != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.AutoOrganize.ToStringFast(),
                value => options.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Carry Chest
        if (isDefault || this.DefaultOptions.CarryChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CarryChest.ToStringFast(),
                value => options.CarryChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Categorize Chest
        if (isDefault || this.DefaultOptions.CategorizeChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChest.ToStringFast(),
                value => options.CategorizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CategorizeChest_Name,
                I18n.Config_CategorizeChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);

            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChestAutomatically.ToStringFast(),
                value => options.CategorizeChestAutomatically = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CategorizeChestAutomatically_Name,
                I18n.Config_CategorizeChestAutomatically_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);

            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChestMethod.ToStringFast(),
                value => options.CategorizeChestMethod = FilterMethodExtensions.TryParse(value, out var filterMethod)
                    ? filterMethod
                    : FilterMethod.Default,
                I18n.Config_CategorizeChestMethod_Name,
                I18n.Config_CategorizeChestMethod_Tooltip,
                FilterMethodExtensions.GetNames(),
                this.localizedTextManager.FormatMethod);
        }

        // Chest Finder
        if (isDefault || this.DefaultOptions.ChestFinder != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ChestFinder.ToStringFast(),
                value => options.ChestFinder = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestFinder_Name,
                I18n.Config_ChestFinder_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Chest Info
        if (isDefault || this.DefaultOptions.ChestInfo != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ChestInfo.ToStringFast(),
                value => options.ChestInfo = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestInfo_Name,
                I18n.Config_ChestInfo_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Collect Items
        if (isDefault || this.DefaultOptions.CollectItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CollectItems.ToStringFast(),
                value => options.CollectItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Configure Chest
        if (isDefault || this.DefaultOptions.ConfigureChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ConfigureChest.ToStringFast(),
                value => options.ConfigureChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ConfigureChest_Name,
                I18n.Config_ConfigureChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Craft from Chest
        if (isDefault || this.DefaultOptions.CraftFromChest != RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
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
                (int)RangeOption.Default,
                (int)RangeOption.World,
                1,
                this.localizedTextManager.Distance);
        }

        // HSL Color Picker
        if (isDefault || this.DefaultOptions.HslColorPicker != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.HslColorPicker.ToStringFast(),
                value => options.HslColorPicker = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_HslColorPicker_Name,
                I18n.Config_HslColorPicker_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Inventory Tabs
        if (isDefault || this.DefaultOptions.InventoryTabs != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.InventoryTabs.ToStringFast(),
                value => options.InventoryTabs = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_InventoryTabs_Name,
                I18n.Config_InventoryTabs_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Open Held Chest
        if (isDefault || this.DefaultOptions.OpenHeldChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.OpenHeldChest.ToStringFast(),
                value => options.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Resize Chest
        var size = (int)options.ResizeChest;
        gmcm.OnFieldChanged(
            this.manifest,
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
            this.manifest,
            () => options.ResizeChestCapacity switch
            {
                -1 => 9,
                _ when size > 1 => Math.Min(9, (int)Math.Ceiling((float)options.ResizeChestCapacity / size)),
                0 => 0,
                9 => 1,
                36 => 2,
                70 => 3,
                _ => Math.Min(9, (int)Math.Ceiling(options.ResizeChestCapacity / 70f)),
            },
            value => options.ResizeChestCapacity = value switch
            {
                9 => -1, 0 => 0, _ when size > 1 => value * size, 1 => 9, 2 => 36, 3 => 70, _ => 70 * value,
            },
            I18n.Config_ResizeChest_Name,
            I18n.Config_ResizeChest_Tooltip,
            0,
            9,
            1,
            value => this.localizedTextManager.FormatCapacity(value, size));

        // Resize Chest Menu
        if (isDefault || this.DefaultOptions.ResizeChest != ChestMenuOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ResizeChest.ToStringFast(),
                value => options.ResizeChest = ChestMenuOptionExtensions.TryParse(value, out var option)
                    ? option
                    : ChestMenuOption.Default,
                I18n.Config_ResizeChestMenu_Name,
                I18n.Config_ResizeChestMenu_Tooltip,
                ChestMenuOptionExtensions.GetNames(),
                this.localizedTextManager.ChestMenuSize,
                nameof(options.ResizeChest));
        }

        // Search Items
        if (isDefault || this.DefaultOptions.SearchItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.SearchItems.ToStringFast(),
                value => options.SearchItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Shop from Chest
        if (isDefault || this.DefaultOptions.ShopFromChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ShopFromChest.ToStringFast(),
                value => options.ShopFromChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ShopFromChest_Name,
                I18n.Config_ShopFromChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Stash to Chest
        if (isDefault || this.DefaultOptions.StashToChest != RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
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
                (int)RangeOption.Default,
                (int)RangeOption.World,
                1,
                this.localizedTextManager.Distance);
        }
    }

    private void AddControls(Controls controls)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

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
            () => controls.FindChest,
            value => controls.FindChest = value,
            I18n.Controls_FindChest_Name,
            I18n.Controls_FindChest_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.OpenFoundChest,
            value => controls.OpenFoundChest = value,
            I18n.Controls_OpenFoundChest_Name,
            I18n.Controls_OpenFoundChest_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.CloseChestFinder,
            value => controls.CloseChestFinder = value,
            I18n.Controls_CloseChestFinder_Name,
            I18n.Controls_CloseChestFinder_Tooltip);

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

        // Inventory Tabs
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.PreviousTab,
            value => controls.PreviousTab = value,
            I18n.Controls_PreviousTab_Name,
            I18n.Controls_PreviousTab_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.NextTab,
            value => controls.NextTab = value,
            I18n.Controls_NextTab_Name,
            I18n.Controls_NextTab_Tooltip);

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
    }

    private void AddTweaks(DefaultConfig config)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

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
            this.localizedTextManager.CarryChestLimit);

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
            this.localizedTextManager.CarryChestLimit);

        // TODO: Move Workbench into an object type config for workbench
        // Craft From Workbench
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CraftFromWorkbenchDistance switch
            {
                _ when config.CraftFromWorkbench is RangeOption.Default => (int)RangeOption.Default,
                _ when config.CraftFromWorkbench is RangeOption.Disabled => (int)RangeOption.Disabled,
                _ when config.CraftFromWorkbench is RangeOption.Inventory => (int)RangeOption.Inventory,
                _ when config.CraftFromWorkbench is RangeOption.World => (int)RangeOption.World,
                >= 2 when config.CraftFromWorkbench is RangeOption.Location => (int)RangeOption.Location
                    + (int)Math.Ceiling(Math.Log2(config.CraftFromWorkbenchDistance))
                    - 1,
                _ when config.CraftFromWorkbench is RangeOption.Location => (int)RangeOption.World - 1,
                _ => (int)RangeOption.Default,
            },
            value =>
            {
                config.CraftFromWorkbenchDistance = value switch
                {
                    (int)RangeOption.Default => 0,
                    (int)RangeOption.Disabled => 0,
                    (int)RangeOption.Inventory => 0,
                    (int)RangeOption.World => 0,
                    (int)RangeOption.World - 1 => -1,
                    >= (int)RangeOption.Location => (int)Math.Pow(2, 1 + value - (int)RangeOption.Location),
                    _ => 0,
                };

                config.CraftFromWorkbench = value switch
                {
                    (int)RangeOption.Default => RangeOption.Default,
                    (int)RangeOption.Disabled => RangeOption.Disabled,
                    (int)RangeOption.Inventory => RangeOption.Inventory,
                    (int)RangeOption.World => RangeOption.World,
                    (int)RangeOption.World - 1 => RangeOption.Location,
                    _ => RangeOption.Location,
                };
            },
            I18n.Config_CraftFromWorkbench_Name,
            I18n.Config_CraftFromWorkbench_Tooltip,
            (int)RangeOption.Default,
            (int)RangeOption.World,
            1,
            this.localizedTextManager.Distance);

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

        // Inventory Tab Method
        gmcm.AddTextOption(
            this.manifest,
            () => config.InventoryTabMethod.ToStringFast(),
            value => config.InventoryTabMethod =
                FilterMethodExtensions.TryParse(value, out var method) ? method : FilterMethod.Default,
            I18n.Config_CategorizeChestMethod_Name,
            I18n.Config_CategorizeChestMethod_Tooltip,
            FilterMethodExtensions.GetNames(),
            this.localizedTextManager.FormatMethod);

        // Lock Item
        gmcm.AddTextOption(
            this.manifest,
            () => config.LockItem.ToStringFast(),
            value => config.LockItem =
                FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_LockItem_Name,
            I18n.Config_LockItem_Tooltip,
            FeatureOptionExtensions.GetNames(),
            this.localizedTextManager.FormatOption);

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
            I18n.Config_CategorizeChestMethod_Name,
            I18n.Config_CategorizeChestMethod_Tooltip,
            FilterMethodExtensions.GetNames(),
            this.localizedTextManager.FormatMethod);

        // Search Tag Symbol
        gmcm.AddTextOption(
            this.manifest,
            () => config.SearchTagSymbol.ToString(),
            value => config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchTagSymbol_Name,
            I18n.Config_SearchTagSymbol_Tooltip);

        // Search Negation Symbol
        gmcm.AddTextOption(
            this.manifest,
            () => config.SearchNegationSymbol.ToString(),
            value => config.SearchNegationSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchNegationSymbol_Name,
            I18n.Config_SearchNegationSymbol_Tooltip);
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
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

    private void InitializeStorageTypes(DefaultConfig config)
    {
        // Initialize Data Types
        config.StorageOptions.TryAdd("BigCraftables", []);
        config.StorageOptions.TryAdd("Buildings", []);
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
                "213",
                new DefaultStorageOptions
                {
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
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
                    CategorizeChestAutomatically = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    ChestInfo = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
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
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    CategorizeChestAutomatically = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    ChestInfo = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Disabled,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize Junimo Hut
        config
            .StorageOptions["Buildings"]
            .TryAdd(
                "Junimo Hut",
                new DefaultStorageOptions
                {
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    CategorizeChestAutomatically = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    ChestInfo = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
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
                    AutoOrganize = FeatureOption.Disabled,
                    CarryChest = FeatureOption.Disabled,
                    CategorizeChest = FeatureOption.Disabled,
                    CategorizeChestAutomatically = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    ChestInfo = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
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
                    CategorizeChestAutomatically = FeatureOption.Disabled,
                    ChestFinder = FeatureOption.Disabled,
                    ChestInfo = FeatureOption.Disabled,
                    CollectItems = FeatureOption.Disabled,
                    ConfigureChest = FeatureOption.Disabled,
                    CraftFromChest = RangeOption.Disabled,
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
                    OpenHeldChest = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Disabled,
                    SearchItems = FeatureOption.Disabled,
                    ShopFromChest = FeatureOption.Disabled,
                    StashToChest = RangeOption.Disabled,
                });

        // Initialize FarmHouse
        config
            .StorageOptions["Locations"]
            .TryAdd(
                "FarmHouse",
                new DefaultStorageOptions
                {
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
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
                    HslColorPicker = FeatureOption.Disabled,
                    InventoryTabs = FeatureOption.Disabled,
                    ResizeChest = ChestMenuOption.Large,
                    ResizeChestCapacity = -1,
                    SearchItems = FeatureOption.Disabled,
                });
    }
}