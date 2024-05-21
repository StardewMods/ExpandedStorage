namespace StardewMods.CustomBush.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.CustomBush.Framework.Models;

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
        : base(eventManager, gameContentHelper, modContentHelper) =>
        this
            .Asset($"{Mod.Id}/Data")
            .Load(static () => new Dictionary<string, CustomBush>(StringComparer.OrdinalIgnoreCase))
            .Edit(AssetHandler.AddIds, (AssetEditPriority)int.MaxValue);

    /// <summary>Gets the data model for all Custom Bush.</summary>
    public Dictionary<string, CustomBush> Data =>
        this.Asset($"{Mod.Id}/Data").Require<Dictionary<string, CustomBush>>();

    private static void AddIds(IAssetData asset)
    {
        var data = asset.AsDictionary<string, CustomBush>().Data;
        foreach (var (key, customBush) in data)
        {
            customBush.Id = key;
        }
    }
}