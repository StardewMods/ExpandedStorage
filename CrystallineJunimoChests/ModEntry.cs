namespace StardewMods.CrystallineJunimoChests;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.CrystallineJunimoChests.Framework.Interfaces;
using StardewMods.CrystallineJunimoChests.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod.Mod
{
    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterSingleton<AssetHandler>();
        this.Container.RegisterSingleton<ChestHandler>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<FauxCoreIntegration>();
        this.Container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.Container.RegisterSingleton<IEventManager, Mod.EventManager>();
        this.Container.RegisterSingleton<Mod.Log>();
        this.Container.RegisterSingleton<ModPatches>();
        this.Container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
    }
}