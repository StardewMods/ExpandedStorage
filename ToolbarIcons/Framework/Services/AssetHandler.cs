namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Models;

/// <inheritdoc />
internal sealed class AssetHandler : BaseAssetHandler
{
    private static readonly InternalIcon[] Icons =
    [
        InternalIcon.StardewAquarium,
        InternalIcon.GenericModConfigMenu,
        InternalIcon.AlwaysScrollMap,
        InternalIcon.ToDew,
        InternalIcon.SpecialOrders,
        InternalIcon.DailyQuests,
        InternalIcon.ToggleCollision,
        InternalIcon.Calendar,
    ];

    private readonly IIconRegistry iconRegistry;
    private readonly IntegrationManager integrationManager;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="integrationManager">Dependency used for managing integrations.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        ContentPatcherIntegration contentPatcherIntegration,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IIconRegistry iconRegistry,
        IntegrationManager integrationManager,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(contentPatcherIntegration, eventManager, gameContentHelper, modContentHelper)
    {
        // Init
        this.iconRegistry = iconRegistry;
        this.integrationManager = integrationManager;
        this
            .Asset($"{Mod.Id}/Data")
            .Load(static () => new Dictionary<string, IntegrationData>(StringComparer.OrdinalIgnoreCase))
            .Watch(this.AddIcons, _ => this.AddIcons());

        themeHelper.AddAsset($"{Mod.Id}/Icons", modContentHelper.Load<IRawTextureData>("assets/icons.png"));

        for (var index = 0; index < AssetHandler.Icons.Length; index++)
        {
            iconRegistry.AddIcon(
                AssetHandler.Icons[index].ToStringFast(),
                $"{Mod.Id}/Icons",
                new Rectangle(16 * (index % 5), 16 * (int)(index / 5f), 16, 16));
        }
    }

    private void AddIcons()
    {
        foreach (var (id, integrationData) in this
            .Asset($"{Mod.Id}/Data")
            .Require<Dictionary<string, IntegrationData>>())
        {
            this.integrationManager.AddIcon(id, integrationData);
        }
    }
}