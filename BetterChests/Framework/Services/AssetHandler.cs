namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;

/// <inheritdoc />
internal sealed class AssetHandler : BaseAssetHandler
{
    private static readonly InternalIcon[] Icons =
    [
        InternalIcon.Clothing,
        InternalIcon.Cooking,
        InternalIcon.Crops,
        InternalIcon.Equipment,
        InternalIcon.Fishing,
        InternalIcon.Materials,
        InternalIcon.Miscellaneous,
        InternalIcon.Seeds,
        InternalIcon.Config,
        InternalIcon.Stash,
        InternalIcon.Craft,
        InternalIcon.Search,
        InternalIcon.Copy,
        InternalIcon.Save,
        InternalIcon.Paste,
        InternalIcon.TransferUp,
        InternalIcon.TransferDown,
        InternalIcon.Hsl,
        InternalIcon.Debug,
        InternalIcon.NoStack,
        InternalIcon.Categorize,
        InternalIcon.Sort,
    ];

    private readonly IModConfig modConfig;
    private HslColor[]? hslColors;
    private Color[]? hslTextureData;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        ContentPatcherIntegration contentPatcherIntegration,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IIconRegistry iconRegistry,
        IModConfig modConfig,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(contentPatcherIntegration, eventManager, gameContentHelper, modContentHelper)
    {
        this.modConfig = modConfig;

        this.Asset($"{Mod.Id}/HueBar").Load<Texture2D>("assets/hue.png");
        this
            .Asset("Data/BigCraftables")
            .Edit(
                this.AddOptions<BigCraftableData>(
                    "BigCraftables",
                    data => data.CustomFields ??= new Dictionary<string, string>()));

        this
            .Asset("Data/Buildings")
            .Edit(
                this.AddOptions<BuildingData>(
                    "Buildings",
                    data => data.CustomFields ??= new Dictionary<string, string>()));

        this
            .Asset("Data/Locations")
            .Edit(
                this.AddOptions<LocationData>(
                    "Locations",
                    data => data.CustomFields ??= new Dictionary<string, string>()));

        themeHelper.AddAsset($"{Mod.Id}/UI", modContentHelper.Load<IRawTextureData>("assets/icons.png"));

        for (var index = 0; index < AssetHandler.Icons.Length; index++)
        {
            iconRegistry.AddIcon(
                AssetHandler.Icons[index].ToStringFast(),
                $"{Mod.Id}/UI",
                new Rectangle(16 * (index % 5), 16 * (int)(index / 5f), 16, 16));
        }

        // Events
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <summary>Gets the hsl colors data.</summary>
    public HslColor[] HslColors =>
        this.hslColors ??= this.HslTextureData.Select(HslColor.FromColor).Distinct().ToArray();

    /// <summary>Gets the hsl texture.</summary>
    public Texture2D HslTexture => this.Asset($"{Mod.Id}/HueBar").Require<Texture2D>();

    private IEnumerable<Color> HslTextureData
    {
        get
        {
            if (this.hslTextureData is not null)
            {
                return this.hslTextureData;
            }

            this.hslTextureData = new Color[this.HslTexture.Width * this.HslTexture.Height];
            this.HslTexture.GetData(this.hslTextureData);
            return this.hslTextureData;
        }
    }

    private Action<IAssetData> AddOptions<T>(string key, Func<T, Dictionary<string, string>> getCustomFields) =>
        asset =>
        {
            if (!this.modConfig.StorageOptions.TryGetValue(key, out var storageTypes))
            {
                return;
            }

            var data = asset.AsDictionary<string, T>().Data;
            foreach (var (storageId, storageOptions) in storageTypes)
            {
                if (!data.TryGetValue(storageId, out var typeData))
                {
                    continue;
                }

                var customFields = getCustomFields(typeData);
                var typeModel = new DictionaryModel(() => customFields);
                var typeOptions = new DictionaryStorageOptions(typeModel);
                storageOptions.CopyTo(typeOptions);
            }
        };

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        foreach (var dataType in e.Config.StorageOptions.Keys)
        {
            this.GameContentHelper.InvalidateCache($"Data/{dataType}");
        }
    }
}