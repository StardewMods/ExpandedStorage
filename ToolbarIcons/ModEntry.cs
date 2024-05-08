namespace StardewMods.ToolbarIcons;

using SimpleInjector;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;
using StardewMods.ToolbarIcons.Framework.UI;
using StardewValley.Menus;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private Container container = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
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

        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<ContentPatcherIntegration>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<FauxCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<IntegrationManager>();
        this.container.RegisterSingleton<ILog, Logger>();
        this.container.RegisterSingleton<IThemeHelper, Themer>();
        this.container.RegisterSingleton<ToolbarManager>();

        this.container.RegisterInstance(new Dictionary<string, ClickableTextureComponent>());
        this.container.RegisterInstance<Func<ToolbarIconOption>>(this.container.GetInstance<ToolbarIconOption>);
        this.container.Register<ToolbarIconOption>();

        this.container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(CjbCheatsMenu),
            typeof(CjbItemSpawner),
            typeof(DailyQuests),
            typeof(DynamicGameAssets),
            typeof(GenericModConfigMenu),
            typeof(SpecialOrders),
            typeof(StardewAquarium),
            typeof(ToDew),
            typeof(ToggleCollision));

        // Verify
        this.container.Verify();

        // Events
        this.container.GetInstance<IEventManager>().Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) =>
        new ToolbarIconsApi(
            mod,
            this.container.GetInstance<IEventManager>(),
            this.container.GetInstance<IGameContentHelper>(),
            this.container.GetInstance<ILog>(),
            this.container.GetInstance<ToolbarManager>());

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        var configManager = this.container.GetInstance<ConfigManager>();
        configManager.Init();
    }
}