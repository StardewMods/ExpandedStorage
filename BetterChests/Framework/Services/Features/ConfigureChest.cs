namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.UI;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature<ConfigureChest>
{
    private readonly AssetHandler assetHandler;
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly PerScreen<BaseDropdown?> dropdown = new();
    private readonly IExpressionHandler expressionHandler;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly MenuHandler menuHandler;
    private readonly UiManager uiManager;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="uiManager">Dependency used for managing ui.</param>
    public ConfigureChest(
        AssetHandler assetHandler,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        ILog log,
        IManifest manifest,
        UiManager uiManager)
        : base(eventManager, log, manifest, configManager)
    {
        this.assetHandler = assetHandler;
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.expressionHandler = expressionHandler;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.uiManager = uiManager;
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
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
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
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuHandler.Top.Container ?? this.menuHandler.Bottom.Container;
        if (container?.ConfigureChest is not FeatureOption.Enabled
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || e.IsSuppressed(SButton.MouseLeft)
            || e.IsSuppressed(SButton.ControllerA)
            || !this.menuHandler.TryGetFocus(this, out var focus))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.dropdown.Value is not null)
        {
            focus.Release();
            this.dropdown.Value.receiveLeftClick(mouseX, mouseY);
            this.inputHelper.Suppress(e.Button);
            return;
        }

        var bounds = this.menuHandler.CurrentMenu switch
        {
            ItemGrabMenu itemGrabMenu => new Rectangle(
                itemGrabMenu.ItemsToGrabMenu.xPositionOnScreen - 100,
                itemGrabMenu.yPositionOnScreen + Game1.tileSize - 48,
                Game1.tileSize,
                Game1.tileSize),
            _ => Rectangle.Empty,
        };

        if (!bounds.Contains(mouseX, mouseY))
        {
            if (this.dropdown.Value is not null)
            {
                this.inputHelper.Suppress(e.Button);
                this.dropdown.Value = null;
            }

            focus.Release();
            return;
        }

        var options = new List<(string Key, string Value)>
        {
            ("configure", I18n.Configure_Options_Name()),
            ("categorize", I18n.Configure_Categorize_Name()),
            ("sort", I18n.Configure_Sorting_Name()),
        };

        this.dropdown.Value = new Dropdown(
            options,
            bounds.X,
            bounds.Bottom,
            value =>
            {
                this.dropdown.Value = null;
                this.ShowMenu(container, value);
            },
            3);
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
        this.ShowMenu(container);
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e) => this.dropdown.Value = null;

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (this.dropdown.Value is not null)
        {
            e.UnHighlight();
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

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e) => this.dropdown.Value?.draw(e.SpriteBatch);

    private void ShowMenu(IStorageContainer container, string? whichMenu = "configure")
    {
        this.lastContainer.Value = container;
        switch (whichMenu)
        {
            case "configure":
                this.containerHandler.Configure(container);
                return;
            case "categorize":
                Game1.activeClickableMenu = new CategorizeMenu(
                    this.assetHandler,
                    container,
                    this.expressionHandler,
                    this.uiManager);

                return;
            case "sort": return;
        }
    }
}