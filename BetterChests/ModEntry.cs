namespace StardewMods.BetterChests;

using SimpleInjector;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterCrafting;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => this.CreateApi(mod);

    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterSingleton<IApiFactory, ApiFactory>();
        container.RegisterSingleton<AssetHandler>();
        container.RegisterSingleton<BetterCraftingIntegration>();
        container.RegisterSingleton<BetterCraftingInventoryProvider>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ConfigManager, ConfigManager>();
        container.RegisterSingleton<ContainerFactory>();
        container.RegisterSingleton<ContainerHandler>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<IExpressionHandler, FauxCoreIntegration>();
        container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        container.RegisterSingleton<GenericModConfigMenuIntegration>();
        container.RegisterSingleton<MenuHandler>();
        container.RegisterSingleton<Localized>();
        container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        container.RegisterSingleton<ProxyChestFactory>();
        container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        container.RegisterSingleton<StatusEffectManager>();
        container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        container.RegisterSingleton<ToolbarIconsIntegration>();
        container.RegisterInstance<Func<IModConfig>>(container.GetInstance<IModConfig>);
        container.Collection.Register<IFeature>(
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