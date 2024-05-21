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
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => this.CreateApi(mod);

    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterSingleton<IApiFactory, ApiFactory>();
        container.RegisterSingleton<AssetHandler>();
        container.RegisterSingleton<ComplexOptionFactory>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        container.RegisterSingleton<FauxCoreIntegration>();
        container.RegisterSingleton<GenericModConfigMenuIntegration>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ConfigManager, ConfigManager>();
        container.RegisterSingleton<IntegrationManager>();
        container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        container.RegisterSingleton<ToolbarManager>();
        container.RegisterInstance(new Dictionary<string, string?>());
        container.Collection.Register<ICustomIntegration>(
            typeof(AlwaysScrollMap),
            typeof(Calendar),
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