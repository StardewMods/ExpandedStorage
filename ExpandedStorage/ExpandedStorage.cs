using ExpandedStorage.Framework.Controllers;
using ExpandedStorage.Framework.Extensions;
using ExpandedStorage.Framework.Patches;
using ExpandedStorage.Framework.Views;
using Common.Integrations.GenericModConfigMenu;
using Common.Integrations.JsonAssets;
using XSAutomate.Common.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;

// ReSharper disable ClassNeverInstantiated.Global

namespace ExpandedStorage
{
    public class ExpandedStorage : Mod
    {
        /// <summary>Controller for Active ItemGrabMenu.</summary>
        internal readonly PerScreen<MenuController> ActiveMenu = new();

        /// <summary>Handled content loaded by Expanded Storage.</summary>
        internal AssetController AssetController;
        internal ChestController ChestController;
        
        /// <summary>The mod configuration.</summary>
        internal ConfigController Config;
        
        /// <summary>Expanded Storage API.</summary>
        internal ExpandedStorageAPI ExpandedStorageAPI;
        internal JsonAssetsIntegration JsonAssets;
        internal GenericModConfigMenuIntegration ModConfigMenu;
        
        /// <summary>Tracks all chests that may be used for vacuum items.</summary>
        internal VacuumChestController VacuumChests;
        
        /// <inheritdoc />
        public override object GetApi()
        {
            return ExpandedStorageAPI;
        }
        
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            JsonAssets = new JsonAssetsIntegration(helper.ModRegistry);
            ModConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);

            Config = Helper.ReadConfig<ConfigController>();
            Config.DefaultStorage.SetDefault();
            Config.Log(Monitor);

            AssetController = new AssetController(this);
            helper.Content.AssetLoaders.Add(AssetController);
            helper.Content.AssetEditors.Add(AssetController);

            ExpandedStorageAPI = new ExpandedStorageAPI(this);
            // Default Exclusions
            ExpandedStorageAPI.DisableWithModData("aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest");
            ExpandedStorageAPI.DisableDrawWithModData("aedenthorn.CustomChestTypes/IsCustomChest");

            VacuumChests = new VacuumChestController(this);
            ChestController = new ChestController(this);

            ItemExtensions.Init(AssetController);
            FarmerExtensions.Init(VacuumChests);
            StorageController.Init(helper.Events);
            HSLColorPicker.Init(helper.Content);

            // Events
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Display.MenuChanged += OnMenuChanged;

            // Harmony Patches
            new Patcher(this).ApplyAll(
                typeof(ItemPatches),
                typeof(ObjectPatches),
                typeof(FarmerPatches),
                typeof(ChestPatches),
                typeof(ItemGrabMenuPatches),
                typeof(InventoryMenuPatches),
                typeof(MenuWithInventoryPatches),
                typeof(DiscreteColorPickerPatches),
                typeof(DebrisPatches),
                typeof(UtilityPatches),
                typeof(ChestsAnywherePatches)
            );
        }
        
        /// <summary>Raised after the game is launched, right before the first update tick.</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Config.RegisterModConfig(this);
        }
        
        /// <summary>Resets scrolling/overlay when chest menu exits or context changes.</summary>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            ActiveMenu.Value?.Dispose();
            if (e.NewMenu is ItemGrabMenu {shippingBin: false} menu)
                ActiveMenu.Value = new MenuController(menu, AssetController, Config, Helper.Events, Helper.Input);
        }
    }
}