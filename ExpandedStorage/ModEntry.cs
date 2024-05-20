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
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    protected override void Init()
    {
        I18n.Init(this.Helper.Translation);
        this.Container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.Container.RegisterSingleton<AssetHandler>();
        this.Container.RegisterSingleton<BetterChestsIntegration>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<IEventManager, EventManager>();
        this.Container.RegisterSingleton<FauxCoreIntegration>();
        this.Container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.Container.RegisterSingleton<Log>();
        this.Container.RegisterSingleton<ModPatches>();
        this.Container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.Container.RegisterSingleton<StorageDataFactory>();
        this.Container.RegisterSingleton<ToolbarIconsIntegration>();
    }
}