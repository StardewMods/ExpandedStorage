namespace StardewMods.BetterChests;

using HarmonyLib;
using SimpleInjector;
using StardewMods.BetterChests.Framework;
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
public sealed class ModEntry : Mod
{
    private Container container = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.container = new Container();

        // Configuration
        this.container.RegisterSingleton(() => new Harmony(this.ModManifest.UniqueID));
        this.container.RegisterInstance(this.Helper);
        this.container.RegisterInstance(this.ModManifest);
        this.container.RegisterInstance(this.Monitor);
        this.container.RegisterInstance(this.Helper.ConsoleCommands);
        this.container.RegisterInstance(this.Helper.Data);
        this.container.RegisterInstance(this.Helper.Events);
        this.container.RegisterInstance(this.Helper.GameContent);
        this.container.RegisterInstance(this.Helper.Input);
        this.container.RegisterInstance(this.Helper.ModContent);
        this.container.RegisterInstance(this.Helper.ModRegistry);
        this.container.RegisterInstance(this.Helper.Reflection);
        this.container.RegisterInstance(this.Helper.Translation);

        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<BetterCraftingIntegration>();
        this.container.RegisterSingleton<BetterCraftingInventoryProvider>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ConfigManager, ConfigManager>();
        this.container.RegisterSingleton<ContainerFactory>();
        this.container.RegisterSingleton<ContainerHandler>();
        this.container.RegisterSingleton<ContentPatcherIntegration>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<IExpressionHandler, FauxCoreIntegration>();
        this.container.RegisterSingleton<IIconRegistry, FauxCoreIntegration>();
        this.container.RegisterSingleton<GenericModConfigMenuIntegration>();
        this.container.RegisterSingleton<MenuHandler>();
        this.container.RegisterSingleton<Localized>();
        this.container.RegisterSingleton<ILog, FauxCoreIntegration>();
        this.container.RegisterSingleton<IPatchManager, FauxCoreIntegration>();
        this.container.RegisterSingleton<ProxyChestFactory>();
        this.container.RegisterSingleton<StatusEffectManager>();
        this.container.RegisterSingleton<IThemeHelper, FauxCoreIntegration>();
        this.container.RegisterSingleton<ToolbarIconsIntegration>();
        this.container.RegisterSingleton<UiManager>();

        this.container.RegisterInstance<Func<IModConfig>>(this.container.GetInstance<IModConfig>);

        this.container.Collection.Register<IFeature>(
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

        // Verify
        this.container.Verify();
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) =>
        new BetterChestsApi(
            mod,
            this.container.GetInstance<ConfigManager>(),
            this.container.GetInstance<ContainerHandler>(),
            this.container.GetInstance<ContainerFactory>());
}