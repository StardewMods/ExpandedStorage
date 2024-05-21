namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
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
        this.Asset($"{Mod.Id}/Data").Load<ColorData[]>("assets/data.json");
        this.Asset("Data/BigCraftables").Edit<BigCraftableData>("256", this.EditJunimoChest, AssetEditPriority.Late);
    }

    /// <summary>Gets the data model.</summary>
    public ColorData[] Data => this.Asset($"{Mod.Id}/Data").Require<ColorData[]>();

    private void EditJunimoChest(BigCraftableData entry)
    {
        entry.SpriteIndex = 0;
        entry.Texture = this.ModContentHelper.GetInternalAssetName("assets/Default.png").Name;
        entry.CustomFields ??= [];
        entry.CustomFields["furyx639.ExpandedStorage/Enabled"] = "true";

        var typeModel = new DictionaryModel(() => entry.CustomFields);
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