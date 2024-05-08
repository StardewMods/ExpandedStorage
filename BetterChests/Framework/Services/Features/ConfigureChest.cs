namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature<ConfigureChest>
{
    private static ConfigureChest instance = null!;
    private readonly AssetHandler assetHandler;

    private readonly PerScreen<ClickableTextureComponent?> configButton = new();
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly MenuHandler menuHandler;
    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ConfigureChest(
        AssetHandler assetHandler,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager)
        : base(eventManager, log, manifest, configManager)
    {
        ConfigureChest.instance = this;
        this.assetHandler = assetHandler;
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.patchManager = patchManager;

        // Patches
        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                AccessTools.DeclaredMethod(
                    typeof(ConfigureChest),
                    nameof(ConfigureChest.ItemGrabMenu_RepositionSideButtons_postfix)),
                PatchType.Postfix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive =>
        this.Config.DefaultOptions.ConfigureChest != FeatureOption.Disabled
        && this.genericModConfigMenuIntegration.IsLoaded;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

        // Patches
        this.patchManager.Patch(this.UniqueId);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

        // Patches
        this.patchManager.Unpatch(this.UniqueId);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        if (ConfigureChest.instance.configButton.Value is null)
        {
            return;
        }

        var configButton = ConfigureChest.instance.configButton.Value;
        if (__instance.allClickableComponents?.Contains(configButton) == false)
        {
            __instance.allClickableComponents.Add(configButton);
        }

        configButton.bounds.Y = 0;
        var buttons =
            new[]
                {
                    __instance.organizeButton,
                    __instance.fillStacksButton,
                    __instance.colorPickerToggleButton,
                    __instance.specialButton,
                    __instance.junimoNoteIcon,
                }
                .Where(component => component is not null)
                .ToList();

        buttons.Add(configButton);
        var stepSize = Game1.tileSize + buttons.Count switch { >= 4 => 8, _ => 16 };
        var yOffset = buttons[0].bounds.Y;

        var xPosition = Math.Max(buttons[0].bounds.X, __instance.okButton.bounds.X);

        for (var index = 0; index < buttons.Count; ++index)
        {
            var button = buttons[index];
            if (index > 0 && buttons.Count > 1)
            {
                button.downNeighborID = buttons[index - 1].myID;
            }

            if (index < buttons.Count - 1 && buttons.Count > 1)
            {
                button.upNeighborID = buttons[index + 1].myID;
            }

            button.bounds.X = xPosition;
            button.bounds.Y = yOffset - (stepSize * index);
        }

        foreach (var component in __instance.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right))
        {
            component.rightNeighborID =
                buttons.MinBy(c => Math.Abs(c.bounds.Center.Y - component.bounds.Center.Y))?.myID ?? -1;
        }
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (this.configButton.Value is null
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || !this.menuHandler.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.lastContainer.Value = this.menuHandler.Top.Container ?? this.menuHandler.Bottom.Container;
        if (this.lastContainer.Value is null)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.ShowMenu();
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !this.Config.Controls.ConfigureChest.JustPressed()
            || (!this.containerFactory.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var container)
                && !this.containerFactory.TryGetOne(Game1.player.currentLocation, e.Cursor.Tile, out container)))
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ConfigureChest);
        this.lastContainer.Value = container;
        this.ShowMenu();
    }

    [Priority(1000)]
    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        Icon? icon;
        switch (e.Parent)
        {
            case ItemGrabMenu itemGrabMenu:
                if (this.menuHandler.Top.Container?.ConfigureChest != FeatureOption.Enabled
                    || !this.assetHandler.Icons.TryGetValue(this.ModId + "/Config", out icon))
                {
                    this.configButton.Value = null;
                    return;
                }

                this.configButton.Value = new ClickableTextureComponent(
                    new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                    this.assetHandler.UiTexture,
                    icon.Area,
                    Game1.pixelZoom)
                {
                    name = this.Id,
                    hoverText = I18n.Button_Configure_Name(),
                    myID = 42_069,
                    region = ItemGrabMenu.region_organizationButtons,
                };

                itemGrabMenu.RepositionSideButtons();
                return;

            case InventoryPage inventoryPage:
                if (this.menuHandler.Bottom.Container?.ConfigureChest != FeatureOption.Enabled
                    || !this.assetHandler.Icons.TryGetValue(this.ModId + "/Config", out icon))
                {
                    this.configButton.Value = null;
                    return;
                }

                this.configButton.Value = new ClickableTextureComponent(
                    new Rectangle(
                        inventoryPage.organizeButton.bounds.X,
                        inventoryPage.organizeButton.bounds.Y - Game1.tileSize - (IClickableMenu.borderWidth / 2),
                        Game1.tileSize,
                        Game1.tileSize),
                    this.assetHandler.UiTexture,
                    icon.Area,
                    Game1.pixelZoom)
                {
                    name = this.Id,
                    hoverText = I18n.Button_Configure_Name(),
                    myID = 42_069,
                    region = ItemGrabMenu.region_organizationButtons,
                };

                inventoryPage.allClickableComponents.Add(this.configButton.Value);
                return;

            case ShopMenu shopMenu:
                if (this.menuHandler.Top.Container?.ConfigureChest != FeatureOption.Enabled
                    || !this.assetHandler.Icons.TryGetValue(this.ModId + "/Config", out icon))
                {
                    this.configButton.Value = null;
                    return;
                }

                this.configButton.Value = new ClickableTextureComponent(
                    new Rectangle(
                        shopMenu.upArrow.bounds.X + Game1.tileSize + (IClickableMenu.borderWidth / 2),
                        shopMenu.upArrow.bounds.Y,
                        Game1.tileSize,
                        Game1.tileSize),
                    this.assetHandler.UiTexture,
                    icon.Area,
                    Game1.pixelZoom)
                {
                    name = this.Id,
                    hoverText = I18n.Button_Configure_Name(),
                    myID = 42_069,
                    region = ItemGrabMenu.region_organizationButtons,
                };

                return;

            default:
                this.configButton.Value = null;
                return;
        }
    }

    private void OnMenuChanged(MenuChangedEventArgs e)
    {
        if (this.lastContainer.Value is null
            || e.OldMenu?.GetType().Name != "SpecificModConfigMenu"
            || e.NewMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            return;
        }

        this.configManager.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            this.lastContainer.Value = null;
            return;
        }

        this.lastContainer.Value.ShowMenu();
        this.lastContainer.Value = null;
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (this.configButton.Value is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.configButton.Value.tryHover(mouseX, mouseY);
        e.SpriteBatch.Draw(
            this.configButton.Value.texture,
            new Vector2(
                this.configButton.Value.bounds.X + (8 * Game1.pixelZoom),
                this.configButton.Value.bounds.Y + (8 * Game1.pixelZoom)),
            new Rectangle(64, 0, 16, 16),
            Color.White,
            0f,
            new Vector2(8, 8),
            this.configButton.Value.scale,
            SpriteEffects.None,
            0.86f);

        this.configButton.Value.draw(e.SpriteBatch);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        switch (this.menuHandler.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                itemGrabMenu.hoverText = this.configButton.Value.hoverText;
                return;

            case InventoryPage inventoryPage:
                inventoryPage.hoverText = this.configButton.Value.hoverText;
                return;

            case ShopMenu shopMenu:
                shopMenu.hoverText = this.configButton.Value.hoverText;
                return;
        }
    }

    private void ShowMenu()
    {
        if (this.lastContainer.Value is not null)
        {
            this.containerHandler.Configure(this.lastContainer.Value);
        }
    }
}