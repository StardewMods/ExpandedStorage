namespace StardewMods.CustomBush.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.CustomBush.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler
{
    private readonly string dataPath;
    private readonly IGameContentHelper gameContentHelper;

    private Dictionary<string, CustomBush>? data;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    public AssetHandler(IGameContentHelper gameContentHelper, IEventManager eventManager)
    {
        this.gameContentHelper = gameContentHelper;
        this.dataPath = Mod.Id + "/Data";
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    /// <summary>Gets the data model for all Custom Bush.</summary>
    public Dictionary<string, CustomBush> Data
    {
        get
        {
            if (this.data is not null)
            {
                return this.data;
            }

            this.data = this.gameContentHelper.Load<Dictionary<string, CustomBush>>(this.dataPath);
            foreach (var (id, customBush) in this.data)
            {
                customBush.Id = id;
            }

            return this.data;
        }
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.dataPath))
        {
            e.LoadFrom(
                static () => new Dictionary<string, CustomBush>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(this.dataPath)))
        {
            this.data = null;
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) => this.data = null;
}