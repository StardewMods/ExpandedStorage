namespace StardewMods.ExpandedStorage;

using HarmonyLib;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ExpandedStorage.Framework.Interfaces;
using StardewMods.ExpandedStorage.Framework.Services;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        container.RegisterSingleton<AssetHandler>();
        container.RegisterSingleton<BetterChestsIntegration>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<FauxCoreIntegration>();
        container.RegisterSingleton<GenericModConfigMenuIntegration>();
        container.RegisterSingleton<ModPatches>();
        container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        container.RegisterSingleton<StorageDataFactory>();
        container.RegisterSingleton<ToolbarIconsIntegration>();
    }
}