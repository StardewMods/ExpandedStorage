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
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterSingleton<AssetHandler>();
        container.RegisterSingleton<CollectService>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ConfigManager, ConfigManager>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<DispenseService>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<FauxCoreIntegration>();
        container.RegisterSingleton<GenericModConfigMenuIntegration>();
        container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        container.RegisterSingleton<ToolbarIconsIntegration>();
    }
}