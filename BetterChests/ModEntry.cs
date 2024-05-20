namespace StardewMods.BetterChests;

using HarmonyLib;
using SimpleInjector;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterCrafting;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
public sealed class ModEntry : Mod.Mod
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
        this.Container.RegisterSingleton<BetterCraftingIntegration>();
        this.Container.RegisterSingleton<BetterCraftingInventoryProvider>();
        this.Container.RegisterSingleton<IModConfig, ConfigManager>();
        this.Container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.Container.RegisterSingleton<ContainerFactory>();
        this.Container.RegisterSingleton<ContainerHandler>();
        this.Container.RegisterSingleton<ContentPatcherIntegration>();
        this.Container.RegisterSingleton<IEventManager, Mod.EventManager>();
        this.Container.RegisterSingleton<IExpressionHandler, FauxCoreIntegration>();
        this.Container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        this.Container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.Container.RegisterSingleton<MenuHandler>();
        this.Container.RegisterSingleton<Localized>();
        this.Container.RegisterSingleton<Mod.Log>();
        this.Container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ProxyChestFactory>();
        this.Container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.Container.RegisterSingleton<StatusEffectManager>();
        this.Container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        this.Container.RegisterSingleton<ToolbarIconsIntegration>();
        this.Container.RegisterInstance<Func<IModConfig>>(this.Container.GetInstance<IModConfig>);
        this.Container.Collection.Register<IFeature>(
            new[]
            {
                typeof(AccessChest),
                typeof(AutoOrganize),
                typeof(CarryChest),
                typeof(CategorizeChest),
                typeof(ChestFinder),
                typeof(CollectItems),
                typeof(ConfigureChest),
                typeof(CraftFromChest),
                typeof(DebugMode),
                typeof(HslColorPicker),
                typeof(InventoryTabs),
                typeof(LockItem),
                typeof(OpenHeldChest),
                typeof(ResizeChest),
                typeof(SearchItems),
                typeof(ShopFromChest),
                typeof(SortInventory),
                typeof(StashToChest),
                typeof(StorageInfo),
            },
            Lifestyle.Singleton);
    }
}