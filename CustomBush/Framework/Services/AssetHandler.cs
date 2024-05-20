namespace StardewMods.CustomBush.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Assets;
using StardewMods.CustomBush.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : Mod.BaseAssetHandler
{
    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// ///
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModContentHelper modContentHelper)
        : base(eventManager, gameContentHelper, modContentHelper) =>
        this.AddAsset(
            $"{Mod.Mod.Id}/Data",
            new ModAsset<Dictionary<string, CustomBush>>(
                static () => new Dictionary<string, CustomBush>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive));

    /// <summary>Gets the data model for all Custom Bush.</summary>
    public Dictionary<string, CustomBush> Data
    {
        get
        {
            var data = this.RequireAsset<Dictionary<string, CustomBush>>($"{Mod.Mod.Id}/Data");
            foreach (var (id, customBush) in data)
            {
                customBush.Id = id;
            }

            return data;
        }
    }
}