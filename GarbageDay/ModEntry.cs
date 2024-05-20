namespace StardewMods.GarbageDay;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;
using StardewMods.GarbageDay.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod.Mod
{
    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterInstance(new Dictionary<string, FoundGarbageCan>());
        this.Container.RegisterSingleton<AssetHandler>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<IEventManager, Mod.EventManager>();
        this.Container.RegisterSingleton<FauxCoreIntegration>();
        this.Container.RegisterSingleton<GarbageCanManager>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.Container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        this.Container.RegisterSingleton<Mod.Log>();
        this.Container.RegisterSingleton<ToolbarIconsIntegration>();
    }
}