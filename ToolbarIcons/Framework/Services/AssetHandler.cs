namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
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

    private readonly IntegrationManager integrationManager;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="integrationManager">Dependency used for managing integrations.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IIconRegistry iconRegistry,
        IntegrationManager integrationManager,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(eventManager, gameContentHelper, modContentHelper)
    {
        // Init
        this.integrationManager = integrationManager;
        this
            .Asset($"{Mod.Id}/Data")
            .Load(static () => new Dictionary<string, IntegrationData>(StringComparer.OrdinalIgnoreCase))
            .Watch(onInvalidated: this.AddIcons);

        themeHelper.AddAsset($"{Mod.Id}/Arrows", modContentHelper.Load<IRawTextureData>("assets/arrows.png"));
        themeHelper.AddAsset($"{Mod.Id}/Icons", modContentHelper.Load<IRawTextureData>("assets/icons.png"));

        for (var index = 0; index < AssetHandler.Icons.Length; index++)
        {
            iconRegistry.AddIcon(
                AssetHandler.Icons[index].ToStringFast(),
                $"{Mod.Id}/Icons",
                new Rectangle(16 * (index % 5), 16 * (int)(index / 5f), 16, 16));
        }
    }

    private void AddIcons(AssetsInvalidatedEventArgs e)
    {
        if (!this.Asset($"{Mod.Id}/Data").TryGet<Dictionary<string, IntegrationData>>(out var data))
        {
            return;
        }

        foreach (var (id, integrationData) in data)
        {
            this.integrationManager.AddIcon(id, integrationData);
        }
    }
}