namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature<ConfigureChest>
{
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IExpressionHandler expressionHandler;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly MenuHandler menuHandler;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public ConfigureChest(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        IManifest manifest,
        IReflectionHelper reflectionHelper)
        : base(eventManager, manifest, configManager)
    {
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.expressionHandler = expressionHandler;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.reflectionHelper = reflectionHelper;
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
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ItemHighlightingEventArgs>(ConfigureChest.OnItemHighlighting);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(ConfigureChest.OnItemHighlighting);
    }

    private static void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (Game1.activeClickableMenu?.GetChildMenu() is Dropdown)
        {
            e.UnHighlight();
        }
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || e.IsSuppressed(e.Button)
            || !this.menuHandler.TryGetFocus(this, out var focus))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        IStorageContainer? container = null;
        ClickableComponent? icon = null;
        if (this.menuHandler.Top.Container?.ConfigureChest is FeatureOption.Enabled
            && this.menuHandler.Top.Icon?.containsPoint(mouseX, mouseY) == true)
        {
            container = this.menuHandler.Top.Container;
            icon = this.menuHandler.Top.Icon;
        }

        if (container is null
            && this.menuHandler.Bottom.Container?.ConfigureChest is FeatureOption.Enabled
            && this.menuHandler.Bottom.Icon?.containsPoint(mouseX, mouseY) == true)
        {
            container = this.menuHandler.Bottom.Container;
            icon = this.menuHandler.Bottom.Icon;
        }

        if (container is null || icon is null)
        {
            focus.Release();
            return;
        }

        var options = new List<KeyValuePair<string, string>>
        {
            new("configure", I18n.Configure_Options_Name()),
            new("categorize", I18n.Configure_Categorize_Name()),
            new("sort", I18n.Configure_Sorting_Name()),
        };

        focus.Release();
        this.inputHelper.Suppress(e.Button);
        Game1.activeClickableMenu?.SetChildMenu(
            new Dropdown(
                this.inputHelper,
                this.reflectionHelper,
                icon,
                options,
                value => this.ShowMenu(container, value),
                3));
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
                    container,
                    this.expressionHandler,
                    this.iconRegistry,
                    this.inputHelper,
                    this.reflectionHelper);

                return;
            case "sort": return;
        }
    }
}