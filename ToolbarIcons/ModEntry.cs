namespace StardewMods.ToolbarIcons;

using SimpleInjector;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewMods.ToolbarIcons.Framework.Services.Factory;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

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
        this.container.RegisterSingleton<ComplexOptionFactory>();
        this.container.RegisterSingleton<ContentPatcherIntegration>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        this.container.RegisterSingleton<FauxCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<IntegrationManager>();
        this.container.RegisterSingleton<Log>();
        this.container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        this.container.RegisterSingleton<ToolbarManager>();

        this.container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(CjbCheatsMenu),
            typeof(CjbItemSpawner),
            typeof(DailyQuests),
            typeof(GenericModConfigMenu),
            typeof(SpecialOrders),
            typeof(StardewAquarium),
            typeof(ToDew),
            typeof(ToggleCollision));

        this.container.RegisterInstance(new Dictionary<string, string?>());

        // Verify
        this.container.Verify();
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) =>
        new ToolbarIconsApi(
            mod,
            this.container.GetInstance<IEventManager>(),
            this.container.GetInstance<IIconRegistry>(),
            this.container.GetInstance<ToolbarManager>());
}