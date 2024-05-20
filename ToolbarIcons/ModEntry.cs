namespace StardewMods.ToolbarIcons;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewMods.ToolbarIcons.Framework.Services.Factory;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;
using StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => this.Container.GetInstance<ApiFactory>().CreateApi(mod);

    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterSingleton<ApiFactory>();
        this.Container.RegisterSingleton<AssetHandler>();
        this.Container.RegisterSingleton<ComplexOptionFactory>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<IEventManager, EventManager>();
        this.Container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        this.Container.RegisterSingleton<FauxCoreIntegration>();
        this.Container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.Container.RegisterSingleton<IntegrationManager>();
        this.Container.RegisterSingleton<Log>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.Container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ToolbarManager>();
        this.Container.RegisterInstance(new Dictionary<string, string?>());
        this.Container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(CjbCheatsMenu),
            typeof(CjbItemSpawner),
            typeof(DailyQuests),
            typeof(GenericModConfigMenu),
            typeof(SpecialOrders),
            typeof(StardewAquarium),
            typeof(ToDew),
            typeof(ToggleCollision));
    }
}