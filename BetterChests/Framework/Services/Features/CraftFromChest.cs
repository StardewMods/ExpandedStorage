namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterCrafting;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>Craft using items from placed chests and chests in the farmer's inventory.</summary>
internal sealed class CraftFromChest : BaseFeature<CraftFromChest>
{
    private static CraftFromChest instance = null!;

    private readonly BetterCraftingIntegration betterCraftingIntegration;
    private readonly BetterCraftingInventoryProvider betterCraftingInventoryProvider;
    private readonly ContainerFactory containerFactory;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="CraftFromChest" /> class.</summary>
    /// <param name="betterCraftingIntegration">Dependency for Better Crafting integration.</param>
    /// <param name="betterCraftingInventoryProvider">Dependency used for providing inventories to Better Crafting.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public CraftFromChest(
        BetterCraftingIntegration betterCraftingIntegration,
        BetterCraftingInventoryProvider betterCraftingInventoryProvider,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(eventManager, modConfig)
    {
        CraftFromChest.instance = this;
        this.betterCraftingIntegration = betterCraftingIntegration;
        this.betterCraftingInventoryProvider = betterCraftingInventoryProvider;
        this.containerFactory = containerFactory;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.toolbarIconsIntegration = toolbarIconsIntegration;

        this.Events.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
    public override bool ShouldBeActive =>
        this.Config.DefaultOptions.CraftFromChest != RangeOption.Disabled && this.betterCraftingIntegration.IsLoaded;

    /// <inheritdoc />
    protected override void Activate()
    {
        if (!this.betterCraftingIntegration.IsLoaded)
        {
            return;
        }

        // Events
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(BuildingContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(ChestContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(FarmerContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(FridgeContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(FurnitureContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(NpcContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.RegisterInventoryProvider(
            typeof(ObjectContainer),
            this.betterCraftingInventoryProvider);

        this.betterCraftingIntegration.Api.MenuPopulateContainers += this.OnMenuPopulateContainers;

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.iconRegistry.RequireIcon(InternalIcon.Craft),
            I18n.Button_CraftFromChest_Name());
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        if (!this.betterCraftingIntegration.IsLoaded)
        {
            return;
        }

        // Events
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);

        // Integrations
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(BuildingContainer));
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(ChestContainer));
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(FarmerContainer));
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(FridgeContainer));
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(FurnitureContainer));
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(NpcContainer));
        this.betterCraftingIntegration.Api.UnregisterInventoryProvider(typeof(ObjectContainer));
        this.betterCraftingIntegration.Api.MenuPopulateContainers -= this.OnMenuPopulateContainers;

        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.Unsubscribe(this.OnIconPressed);
        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.iconRegistry.RequireIcon(InternalIcon.Craft));
    }

    private static bool CookingPredicate(IStorageContainer container) =>
        container is not FarmerContainer
        && container.CookFromChest is not RangeOption.Disabled
        && container.Items.Count > 0
        && !CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
        && !(CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
            && Game1.player.currentLocation is MineShaft)
        && container.CookFromChest.WithinRange(-1, container.Location, container.TileLocation);

    private static bool DefaultPredicate(IStorageContainer container) =>
        container is not FarmerContainer
        && container.CraftFromChest is not RangeOption.Disabled
        && container.Items.Count > 0
        && !CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
        && !(CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
            && Game1.player.currentLocation is MineShaft)
        && container.CraftFromChest.WithinRange(
            container.CraftFromChestDistance,
            container.Location,
            container.TileLocation);

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.Controls.OpenCrafting.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.OpenCrafting);
        this.betterCraftingIntegration.Api!.OpenCraftingMenu(
            false,
            true,
            Game1.player.currentLocation,
            Game1.player.Tile,
            null,
            false);
    }

    private void OnGameLaunched(GameLaunchedEventArgs obj)
    {
        if (!this.betterCraftingIntegration.IsLoaded)
        {
            Log.Warn("Better Crafting is not loaded. CraftFromChest will not be active.");
        }
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.iconRegistry.RequireIcon(InternalIcon.Craft).Id)
        {
            this.betterCraftingIntegration.Api!.OpenCraftingMenu(
                false,
                true,
                Game1.player.currentLocation,
                Game1.player.Tile,
                null,
                false);
        }
    }

    private void OnMenuPopulateContainers(IPopulateContainersEvent e)
    {
        e.DisableDiscovery = true;
        var location = e.Menu.Location ?? Game1.player.currentLocation;
        var position = e.Menu.Position ?? Game1.player.Tile;
        var predicate = CraftFromChest.DefaultPredicate;

        if (location.Objects.TryGetValue(position, out var obj) && obj is Workbench)
        {
            predicate = container => container is not FarmerContainer
                && container.CraftFromChest is not RangeOption.Disabled
                && !CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains(
                    Game1.player.currentLocation.Name)
                && !(CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
                    && Game1.player.currentLocation is MineShaft)
                && container.CraftFromChest.WithinRange(
                    container.CraftFromChestDistance,
                    container.Location,
                    container.TileLocation);
        }
        else if (location.GetFridgePosition()?.ToVector2().Equals(position) == true)
        {
            predicate = CraftFromChest.CookingPredicate;
        }

        var containers = this.containerFactory.GetAll(predicate).ToList();
        foreach (var container in containers)
        {
            e.Containers.Add(new Tuple<object, GameLocation?>(container, container.Location));
        }
    }
}