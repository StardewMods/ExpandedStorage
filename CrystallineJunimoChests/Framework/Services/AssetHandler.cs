namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Assets;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.CrystallineJunimoChests.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <inheritdoc />
internal sealed class AssetHandler : BaseAssetHandler
{
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
        this.AddAsset($"{Mod.Id}/Data", new ModAsset<ColorData[]>("assets/data.json", AssetLoadPriority.Exclusive));
        this.AddAsset("Data/BigCraftables", new AssetEditor(this.EditBigCraftables, AssetEditPriority.Late));
    }

    /// <summary>Gets the data model.</summary>
    public ColorData[] Data => this.RequireAsset<ColorData[]>($"{Mod.Id}/Data");

    private void EditBigCraftables(IAssetData asset)
    {
        var data = asset.AsDictionary<string, BigCraftableData>().Data;
        if (!data.TryGetValue("256", out var bigCraftableData))
        {
            return;
        }

        bigCraftableData.SpriteIndex = 0;
        bigCraftableData.Texture = this.ModContentHelper.GetInternalAssetName("assets/Default.png").Name;
        bigCraftableData.CustomFields ??= [];
        bigCraftableData.CustomFields["furyx639.ExpandedStorage/Enabled"] = "true";

        var typeModel = new DictionaryModel(() => bigCraftableData.CustomFields);
        var storageData = new StorageData(typeModel);
        storageData.Frames = 5;
        storageData.GlobalInventoryId = "JunimoChest";
        storageData.PlayerColor = true;

        var storageOptions = new StorageOptions(typeModel);
        storageOptions.HslColorPicker = FeatureOption.Disabled;
        storageOptions.InventoryTabs = FeatureOption.Disabled;
        storageOptions.ResizeChest = ChestMenuOption.Disabled;
        storageOptions.ResizeChestCapacity = 0;
    }
}