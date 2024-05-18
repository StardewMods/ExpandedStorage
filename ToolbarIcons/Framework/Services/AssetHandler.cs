namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : GenericBaseService<AssetHandler>
{
    private readonly string dataPath;
    private readonly IGameContentHelper gameContentHelper;
    private readonly IIconRegistry iconRegistry;
    private readonly IntegrationManager integrationManager;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="integrationManager">Dependency used for managing integrations.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IIconRegistry iconRegistry,
        IntegrationManager integrationManager,
        ILog log,
        IManifest manifest,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.gameContentHelper = gameContentHelper;
        this.iconRegistry = iconRegistry;
        this.integrationManager = integrationManager;
        this.dataPath = this.ModId + "/Data";

        themeHelper.AddAsset(this.ModId + "/Arrows", modContentHelper.Load<IRawTextureData>("assets/arrows.png"));
        themeHelper.AddAsset(this.ModId + "/Icons", modContentHelper.Load<IRawTextureData>("assets/icons.png"));

        // Events
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.dataPath))
        {
            e.LoadFrom(static () => new Dictionary<string, IntegrationData>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e)
    {
        var data = this.gameContentHelper.Load<Dictionary<string, IntegrationData>>(this.dataPath);
        foreach (var (id, integrationData) in data)
        {
            this.integrationManager.AddIcon(id, integrationData);
        }
    }

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        var icons = new[]
        {
            InternalIcon.StardewAquarium,
            InternalIcon.GenericModConfigMenu,
            InternalIcon.AlwaysScrollMap,
            InternalIcon.ToDew,
            InternalIcon.SpecialOrders,
            InternalIcon.DailyQuests,
            InternalIcon.ToggleCollision,
        };

        for (var index = 0; index < icons.Length; index++)
        {
            this.iconRegistry.AddIcon(
                icons[index].ToStringFast(),
                $"{this.ModId}/Icons",
                new Rectangle(16 * (index % 5), 16 * (int)(index / 5f), 16, 16));
        }
    }
}