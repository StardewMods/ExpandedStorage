﻿namespace StardewMods.ToolbarIcons;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewMods.ToolbarIcons.Framework.Services.Integrations;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;
using StardewValley.Menus;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private Container container;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod)
    {
        var customEvents = this.container.GetInstance<EventsManager>();
        var toolbar = this.container.GetInstance<ToolbarManager>();
        return new ToolbarIconsApi(mod, customEvents, toolbar);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Init
        this.container = new Container();

        // Configuration
        this.container.RegisterInstance(this.Helper);
        this.container.RegisterInstance(this.ModManifest);
        this.container.RegisterInstance(this.Monitor);
        this.container.RegisterInstance(this.Helper.Data);
        this.container.RegisterInstance(this.Helper.Events);
        this.container.RegisterInstance(this.Helper.GameContent);
        this.container.RegisterInstance(this.Helper.Input);
        this.container.RegisterInstance(this.Helper.ModContent);
        this.container.RegisterInstance(this.Helper.ModRegistry);
        this.container.RegisterInstance(this.Helper.Reflection);
        this.container.RegisterInstance(this.Helper.Translation);
        this.container.RegisterInstance(this.Helper.ReadConfig<ModConfig>());
        this.container.RegisterInstance(new Dictionary<string, ClickableTextureComponent>());
        this.container.RegisterSingleton<FuryCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<ConfigMenuManager>();
        this.container.RegisterSingleton<EventsManager>();
        this.container.RegisterSingleton<IntegrationManager>();
        this.container.RegisterSingleton<ToolbarManager>();
        this.container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(CjbCheatsMenu),
            typeof(CjbItemSpawner),
            typeof(DailyQuests),
            typeof(DynamicGameAssets),
            typeof(GenericModConfigMenu),
            typeof(SpecialOrders),
            typeof(StardewAquarium),
            typeof(ToDew));

        this.container.RegisterSingleton(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                var monitor = this.container.GetInstance<IMonitor>();
                return furyCore.Api!.CreateLogService(monitor);
            });

        this.container.RegisterSingleton(
            () =>
            {
                var furyCore = this.container.GetInstance<FuryCoreIntegration>();
                return furyCore.Api!.CreateThemingService();
            });

        this.container.Verify();
    }
}