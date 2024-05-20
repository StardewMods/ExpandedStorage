namespace StardewMods.FauxCore;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod.Mod
{
    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => this.Container.GetInstance<ApiFactory>().CreateApi(mod);

    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterSingleton<ApiFactory>();
        this.Container.RegisterSingleton<IAssetHandler, AssetHandler>();
        this.Container.RegisterSingleton<Mod.CacheManager>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<IEventManager, Mod.EventManager>();
        this.Container.RegisterSingleton<IExpressionHandler, ExpressionHandler>();
        this.Container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.Container.RegisterSingleton<Mod.Log>();
        this.Container.RegisterSingleton<ISimpleLogging, SimpleLogging>();
        this.Container.RegisterSingleton<ThemeHelper>();
        this.Container.RegisterSingleton<IThemeHelper, ThemeHelper>();
    }
}