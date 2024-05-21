namespace StardewMods.CrystallineJunimoChests;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.CrystallineJunimoChests.Framework.Interfaces;
using StardewMods.CrystallineJunimoChests.Framework.Services;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterSingleton<AssetHandler>();
        container.RegisterSingleton<ChestHandler>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ConfigManager, ConfigManager>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<FauxCoreIntegration>();
        container.RegisterSingleton<GenericModConfigMenuIntegration>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
    }
}