namespace StardewMods.CustomBush;

using HarmonyLib;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.CustomBush.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => this.Container.GetInstance<ApiFactory>().CreateApi(mod);

    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.Container.RegisterSingleton<ApiFactory>();
        this.Container.RegisterSingleton<AssetHandler>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<IEventManager, EventManager>();
        this.Container.RegisterSingleton<FauxCoreIntegration>();
        this.Container.RegisterSingleton<Log>();
        this.Container.RegisterSingleton<ModPatches>();
        this.Container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
    }
}