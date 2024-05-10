namespace StardewMods.CustomBush;

using HarmonyLib;
using SimpleInjector;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.CustomBush.Framework;
using StardewMods.CustomBush.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private Container container = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        this.container = new Container();

        // Configuration
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
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
        this.container.RegisterSingleton<ILog, FauxCoreIntegration>();
        this.container.RegisterSingleton<ModPatches>();
        this.container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();

        // Verify
        this.container.Verify();
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) =>
        new CustomBushApi(
            this.container.GetInstance<AssetHandler>(),
            this.container.GetInstance<ModPatches>(),
            mod,
            this.container.GetInstance<ILog>());
}