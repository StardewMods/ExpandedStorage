namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IDataHelper dataHelper;
    private readonly IGameContentHelper gameContentHelper;
    private readonly string hslTexturePath;
    private readonly Lazy<IManagedTexture> icons;
    private readonly IModConfig modConfig;
    private readonly string tabDataPath;
    private readonly Lazy<IManagedTexture> tabs;
    private readonly string tabTexturePath;

    private HslColor[]? hslColors;
    private Texture2D? hslTexture;
    private Color[]? hslTextureData;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IDataHelper dataHelper,
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.dataHelper = dataHelper;
        this.gameContentHelper = gameContentHelper;
        this.modConfig = modConfig;
        this.hslTexturePath = this.ModId + "/HueBar";
        this.tabDataPath = this.ModId + "/Tabs";
        this.tabTexturePath = this.ModId + "/Tabs/Texture";

        this.icons = new Lazy<IManagedTexture>(
            () => themeHelper.AddAsset(
                this.ModId + "/Icons",
                modContentHelper.Load<IRawTextureData>("assets/icons.png")));

        this.tabs = new Lazy<IManagedTexture>(
            () => themeHelper.AddAsset(this.tabTexturePath, modContentHelper.Load<IRawTextureData>("assets/tabs.png")));

        // Events
        eventSubscriber.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventSubscriber.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <summary>Gets the hsl colors data.</summary>
    public HslColor[] HslColors
    {
        get
        {
            if (this.hslTextureData is not null)
            {
                return this.hslColors ??= this.hslTextureData.Select(HslColor.FromColor).Distinct().ToArray();
            }

            this.hslTextureData = new Color[this.HslTexture.Width * this.HslTexture.Height];
            this.HslTexture.GetData(this.hslTextureData);
            return this.hslColors ??= this.hslTextureData.Select(HslColor.FromColor).Distinct().ToArray();
        }
    }

    /// <summary>Gets the hsl texture.</summary>
    public Texture2D HslTexture => this.hslTexture ??= this.gameContentHelper.Load<Texture2D>(this.hslTexturePath);

    /// <summary>Gets the managed icons texture.</summary>
    public IManagedTexture Icons => this.icons.Value;

    /// <summary>Gets the managed tabs texture.</summary>
    public IManagedTexture Tabs => this.tabs.Value;

    /// <summary>Gets the tab data.</summary>
    public Dictionary<string, InventoryTabData> TabData =>
        this.gameContentHelper.Load<Dictionary<string, InventoryTabData>>(this.tabDataPath);

    private void OnGameLaunched(GameLaunchedEventArgs e) => _ = this.Tabs.Value;

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.hslTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.tabDataPath))
        {
            e.LoadFrom(this.GetTabData, AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("Data/BigCraftables")
            && this.modConfig.StorageOptions.TryGetValue("BigCraftables", out var storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var bigCraftableData))
                        {
                            continue;
                        }

                        bigCraftableData.CustomFields ??= new Dictionary<string, string>();
                        var customFieldStorageOptions =
                            new CustomFieldsStorageOptions(_ => bigCraftableData.CustomFields);

                        var temporaryStorageOptions = new TemporaryStorageOptions(
                            customFieldStorageOptions,
                            storageOptions);

                        temporaryStorageOptions.Reset();
                        temporaryStorageOptions.Save();
                    }
                });

            return;
        }

        if (e.Name.IsEquivalentTo("Data/Buildings")
            && this.modConfig.StorageOptions.TryGetValue("Buildings", out storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BuildingData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var buildingData))
                        {
                            continue;
                        }

                        buildingData.CustomFields ??= new Dictionary<string, string>();
                        var customFieldStorageOptions = new CustomFieldsStorageOptions(_ => buildingData.CustomFields);
                        var temporaryStorageOptions = new TemporaryStorageOptions(
                            customFieldStorageOptions,
                            storageOptions);

                        temporaryStorageOptions.Reset();
                        temporaryStorageOptions.Save();
                    }
                });

            return;
        }

        if (e.Name.IsEquivalentTo("Data/Locations")
            && this.modConfig.StorageOptions.TryGetValue("Locations", out storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, LocationData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var locationData))
                        {
                            continue;
                        }

                        locationData.CustomFields ??= new Dictionary<string, string>();
                        var customFieldStorageOptions = new CustomFieldsStorageOptions(_ => locationData.CustomFields);
                        var temporaryStorageOptions = new TemporaryStorageOptions(
                            customFieldStorageOptions,
                            storageOptions);

                        temporaryStorageOptions.Reset();
                        temporaryStorageOptions.Save();
                    }
                });
        }
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        foreach (var dataType in e.Config.StorageOptions.Keys)
        {
            this.gameContentHelper.InvalidateCache($"Data/{dataType}");
        }
    }

    private Dictionary<string, InventoryTabData> GetTabData()
    {
        Dictionary<string, InventoryTabData>? tabData;
        try
        {
            tabData = this.dataHelper.ReadJsonFile<Dictionary<string, InventoryTabData>>("assets/tabs.json");
        }
        catch (Exception)
        {
            tabData = null;
        }

        if (tabData is not null && tabData.Any())
        {
            return tabData;
        }

        tabData = new Dictionary<string, InventoryTabData>
        {
            {
                "Clothing",
                new InventoryTabData(
                    "Clothing",
                    this.tabTexturePath,
                    2,
                    ["category_clothing", "category_boots", "category_hat"])
            },
            {
                "Cooking",
                new InventoryTabData(
                    "Cooking",
                    this.tabTexturePath,
                    3,
                    [
                        "category_syrup",
                        "category_artisan_goods",
                        "category_ingredients",
                        "category_sell_at_pierres_and_marnies",
                        "category_sell_at_pierres",
                        "category_meat",
                        "category_cooking",
                        "category_milk",
                        "category_egg",
                    ])
            },
            {
                "Crops",
                new InventoryTabData(
                    "Crops",
                    this.tabTexturePath,
                    4,
                    ["category_greens", "category_flowers", "category_fruits", "category_vegetable"])
            },
            {
                "Equipment",
                new InventoryTabData(
                    "Equipment",
                    this.tabTexturePath,
                    5,
                    ["category_equipment", "category_ring", "category_tool", "category_weapon"])
            },
            {
                "Fishing",
                new InventoryTabData(
                    "Fishing",
                    this.tabTexturePath,
                    6,
                    ["category_bait", "category_fish", "category_tackle", "category_sell_at_fish_shop"])
            },
            {
                "Materials",
                new InventoryTabData(
                    "Materials",
                    this.tabTexturePath,
                    7,
                    [
                        "category_monster_loot",
                        "category_metal_resources",
                        "category_building_resources",
                        "category_minerals",
                        "category_crafting",
                        "category_gem",
                    ])
            },
            {
                "Misc",
                new InventoryTabData(
                    "Misc",
                    this.tabTexturePath,
                    8,
                    ["category_big_craftable", "category_furniture", "category_junk"])
            },
            {
                "Seeds",
                new InventoryTabData("Seeds", this.tabTexturePath, 9, ["category_seeds", "category_fertilizer"])
            },
        };

        this.dataHelper.WriteJsonFile("assets/tabs.json", tabData);
        return tabData;
    }
}