namespace StardewMods.EasyAccess;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.EasyAccess.Framework.Interfaces;
using StardewMods.EasyAccess.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterSingleton<AssetHandler>();
        this.Container.RegisterSingleton<CollectService>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<DispenseService>();
        this.Container.RegisterSingleton<IEventManager, EventManager>();
        this.Container.RegisterSingleton<FauxCoreIntegration>();
        this.Container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.Container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        this.Container.RegisterSingleton<Log>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.Container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ToolbarIconsIntegration>();
    }
}