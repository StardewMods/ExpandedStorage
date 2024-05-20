namespace StardewMods.ExpandedStorage.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Assets;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.ExpandedStorage.Framework.Interfaces;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <inheritdoc />
internal sealed class AssetHandler : BaseAssetHandler
{
    private readonly IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModConfig modConfig,
        IModContentHelper modContentHelper)
        : base(eventManager, gameContentHelper, modContentHelper)
    {
        this.modConfig = modConfig;
        this.AddAsset("Data/BigCraftables", new AssetEditor(this.AddOptions, AssetEditPriority.Late));
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    private void AddOptions(IAssetData asset)
    {
        var bigCraftableData = asset.AsDictionary<string, BigCraftableData>().Data;
        foreach (var (id, data) in bigCraftableData)
        {
            if (!this.modConfig.StorageOptions.TryGetValue(id, out var storageOptions))
            {
                continue;
            }

            data.CustomFields ??= new Dictionary<string, string>();
            var typeModel = new DictionaryModel(() => data.CustomFields);
            var typeOptions = new StorageOptions(typeModel);
            storageOptions.CopyTo(typeOptions);
        }
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e) =>
        this.GameContentHelper.InvalidateCache("Data/BigCraftables");
}