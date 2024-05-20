namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Assets;
using StardewMods.CrystallineJunimoChests.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <summary>Handles modification and manipulation of assets in the game.</summary>
internal sealed class AssetHandler : Mod.BaseAssetHandler
{
    private const string BetterChestPrefix = "furyx639.BetterChests/";

    private readonly IReadOnlyDictionary<string, string> customFieldData = new Dictionary<string, string>
    {
        { AssetHandler.BetterChestPrefix + "HslColorPicker", "Disabled" },
        { AssetHandler.BetterChestPrefix + "InventoryTabs", "Disabled" },
        { AssetHandler.BetterChestPrefix + "ResizeChest", "Small" },
        { AssetHandler.BetterChestPrefix + "ResizeChestCapacity", "9" },
    };

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModContentHelper modContentHelper)
        : base(eventManager, gameContentHelper, modContentHelper)
    {
        this.AddAsset($"{Mod.Mod.Id}/Data", new ModAsset<DataModel>("assets/data.json", AssetLoadPriority.Exclusive));
        this.AddAsset(
            $"{Mod.Mod.Id}/Texture",
            new ModAsset<Texture2D>("assets/texture.png", AssetLoadPriority.Exclusive));

        this.AddAsset("Data/BigCraftables", new AssetEditor(this.EditBigCraftables, AssetEditPriority.Early));
    }

    /// <summary>Gets the data model.</summary>
    public DataModel Data => this.RequireAsset<DataModel>($"{Mod.Mod.Id}/Data");

    /// <summary>Gets the texture.</summary>
    public Texture2D Texture => this.RequireAsset<Texture2D>($"{Mod.Mod.Id}/Texture");

    private void EditBigCraftables(IAssetData asset)
    {
        var data = asset.AsDictionary<string, BigCraftableData>().Data;
        if (!data.TryGetValue("256", out var bigCraftableData))
        {
            return;
        }

        bigCraftableData.CustomFields ??= [];
        foreach (var (key, value) in this.customFieldData)
        {
            bigCraftableData.CustomFields[key] = value;
        }
    }
}