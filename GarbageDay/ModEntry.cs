namespace StardewMods.GarbageDay;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;
using StardewMods.GarbageDay.Framework.Services;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterInstance(new Dictionary<string, FoundGarbageCan>());
        container.RegisterSingleton<AssetHandler>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<FauxCoreIntegration>();
        container.RegisterSingleton<GarbageCanManager>();
        container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        container.RegisterSingleton<ToolbarIconsIntegration>();
    }
}