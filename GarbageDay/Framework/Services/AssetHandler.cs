namespace StardewMods.GarbageDay.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;
using StardewValley.GameData.BigCraftables;
using xTile;
using xTile.Dimensions;

/// <summary>Handles modification and manipulation of assets in the game.</summary>
internal sealed class AssetHandler : BaseService
{
    /// <summary>The game path where the big craftable data is stored.</summary>
    private const string BigCraftablePath = "Data/BigCraftables";

    /// <summary>The game path where the garbage can data is stored.</summary>
    private const string GarbageCanPath = "Data/GarbageCans";

    private readonly HashSet<string> invalidGarbageCans = [];
    private readonly string itemId;
    private readonly IModConfig modConfig;
    private readonly IModContentHelper modContentHelper;
    private readonly string qualifiedItemId;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    public AssetHandler(
        IEventManager eventManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModContentHelper modContentHelper)
        : base(log, manifest)
    {
        // Init
        this.IconTexturePath = this.ModId + "/Icons";
        this.itemId = this.ModId + "/GarbageCan";
        this.qualifiedItemId = "(BC)" + this.itemId;
        this.modConfig = modConfig;
        this.modContentHelper = modContentHelper;

        // Events
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the found garbage cans.</summary>
    public Dictionary<string, FoundGarbageCan> FoundGarbageCans { get; } = [];

    /// <summary>Gets a new Garbage Can instance.</summary>
    public SObject GarbageCan => (SObject)ItemRegistry.Create(this.qualifiedItemId);

    /// <summary>Gets the icon texture path.</summary>
    public string IconTexturePath { get; }

    /// <summary>Invalidates a garbage can.</summary>
    /// <param name="whichCan">The name of the garbage can to invalidate.</param>
    public void InvalidateGarbageCan(string whichCan) => this.invalidGarbageCans.Add(whichCan);

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.IconTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        // Load Garbage Can Object
        if (e.NameWithoutLocale.IsEquivalentTo(AssetHandler.BigCraftablePath))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;
                    var bigCraftableData = new BigCraftableData
                    {
                        Name = "Garbage Can",
                        DisplayName = I18n.GarbageCan_Name(),
                        Description = I18n.GarbageCan_Description(),
                        Fragility = 2,
                        IsLamp = false,
                        Texture = this.modContentHelper.GetInternalAssetName("assets/GarbageCan.png").Name,
                        CustomFields = new Dictionary<string, string>
                        {
                            { "furyx639.ExpandedStorage/Enabled", "true" },
                            { "furyx639.ExpandedStorage/Frames", "3" },
                            { "furyx639.ExpandedStorage/CloseNearbySound", "trashcanlid" },
                            { "furyx639.ExpandedStorage/OpenNearby", "true" },
                            { "furyx639.ExpandedStorage/OpenNearbySound", "trashcanlid" },
                            { "furyx639.ExpandedStorage/OpenSound", "trashcan" },
                            { "furyx639.ExpandedStorage/PlayerColor", "true" },
                            { "furyx639.BetterChests/AutoOrganize", "Disabled" },
                            { "furyx639.BetterChests/CarryChest", "Disabled" },
                            { "furyx639.BetterChests/CategorizeChest", "Disabled" },
                            { "furyx639.BetterChests/CollectItems", "Disabled" },
                            { "furyx639.BetterChests/ConfigureChest", "Disabled" },
                            { "furyx639.BetterChests/CookFromChest", "Disabled" },
                            { "furyx639.BetterChests/CraftFromChest", "Disabled" },
                            { "furyx639.BetterChests/HslColorPicker", "Disabled" },
                            { "furyx639.BetterChests/InventoryTabs", "Disabled" },
                            { "furyx639.BetterChests/OpenHeldChest", "Disabled" },
                            { "furyx639.BetterChests/ResizeChest", "Small" },
                            { "furyx639.BetterChests/ResizeChestCapacity", "9" },
                            { "furyx639.BetterChests/SearchItems", "Disabled" },
                            { "furyx639.BetterChests/StashToChest", "Disabled" },
                            { "furyx639.BetterChests/StorageInfo", "Disabled" },
                        },
                    };

                    data.Add(this.itemId, bigCraftableData);
                });

            return;
        }

        if (e.DataType != typeof(Map))
        {
            return;
        }

        e.Edit(
            asset =>
            {
                var map = asset.AsMap().Data;
                for (var x = 0; x < map.Layers[0].LayerWidth; ++x)
                {
                    for (var y = 0; y < map.Layers[0].LayerHeight; ++y)
                    {
                        var layer = map.GetLayer("Buildings");
                        var tile = layer.PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile is null
                            || !tile.Properties.TryGetValue("Action", out var property)
                            || string.IsNullOrWhiteSpace(property))
                        {
                            continue;
                        }

                        var parts = ArgUtility.SplitBySpace(property);
                        if (parts.Length < 2
                            || !parts[0].Equals("Garbage", StringComparison.OrdinalIgnoreCase)
                            || string.IsNullOrWhiteSpace(parts[1])
                            || !this.TryAddFound(parts[1], asset.NameWithoutLocale, x, y))
                        {
                            continue;
                        }

                        this.Log.Trace("Garbage Can found on map: {0}", parts[1]);

                        // Remove base tile
                        layer.Tiles[x, y] = null;

                        // Remove Lid tile
                        layer = map.GetLayer("Front");
                        layer.Tiles[x, y - 1] = null;

                        // Add NoPath to tile
                        map
                            .GetLayer("Back")
                            .PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size)
                            ?.Properties.Add("NoPath", string.Empty);
                    }
                }
            },
            (AssetEditPriority)int.MaxValue);
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(AssetHandler.GarbageCanPath)))
        {
            this.FoundGarbageCans.Clear();
        }
    }

    private bool TryAddFound(string whichCan, IAssetName assetName, int x, int y)
    {
        if (this.FoundGarbageCans.ContainsKey(whichCan))
        {
            return true;
        }

        if (this.invalidGarbageCans.Contains(whichCan))
        {
            return false;
        }

        if (!DataLoader.GarbageCans(Game1.content).GarbageCans.TryGetValue(whichCan, out var garbageCanData))
        {
            return false;
        }

        if (!this.modConfig.OnByDefault && garbageCanData.CustomFields?.GetBool(this.ModId + "/Enabled") != true)
        {
            return false;
        }

        this.FoundGarbageCans.Add(whichCan, new FoundGarbageCan(whichCan, assetName, x, y));
        return true;
    }
}