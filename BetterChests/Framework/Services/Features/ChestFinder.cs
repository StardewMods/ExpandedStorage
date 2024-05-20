namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Overlays;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <summary>Search for which chests have the item you're looking for.</summary>
internal sealed class ChestFinder : BaseFeature<ChestFinder>
{
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<int> currentIndex = new();
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<List<Pointer>> pointers = new(() => []);
    private readonly PerScreen<IExpression?> searchExpression = new();
    private readonly PerScreen<string> searchText = new(() => string.Empty);
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="ChestFinder" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public ChestFinder(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(eventManager, modConfig)
    {
        this.containerFactory = containerFactory;
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.ChestFinder != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
        this.Events.Subscribe<WarpedEventArgs>(this.OnWarped);

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.iconRegistry.RequireIcon(InternalIcon.Search),
            I18n.Button_FindChest_Name());
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
        this.Events.Unsubscribe<WarpedEventArgs>(this.OnWarped);

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.Unsubscribe(this.OnIconPressed);
        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.iconRegistry.RequireIcon(InternalIcon.Search));
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        // Activate Search Bar
        if (Context.IsPlayerFree
            && Game1.displayHUD
            && this.menuHandler.CurrentMenu is null
            && this.Config.Controls.ToggleSearch.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleSearch);
            this.OpenSearchBar();
            return;
        }

        if (this.menuHandler.CurrentMenu is not SearchOverlay)
        {
            return;
        }

        // Open Found Chest
        if (this.pointers.Value.Any() && this.Config.Controls.OpenFoundChest.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.OpenFoundChest);
            var container = this.pointers.Value.First().Container;
            container.Mutex?.RequestLock(
                () =>
                {
                    container.ShowMenu();
                });

            return;
        }

        // Clear Search
        if (this.Config.Controls.ClearSearch.JustPressed())
        {
            this.searchText.Value = string.Empty;
            this.searchExpression.Value = null;
            this.Events.Publish(new SearchChangedEventArgs(string.Empty, null));
        }
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.iconRegistry.RequireIcon(InternalIcon.Search).Id)
        {
            this.OpenSearchBar();
        }
    }

    private void OnMenuChanged(MenuChangedEventArgs e) => this.ReinitializePointers();

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (this.menuHandler.CurrentMenu is not SearchOverlay && (!Game1.displayHUD || !Context.IsPlayerFree))
        {
            return;
        }

        foreach (var pointer in this.pointers.Value)
        {
            pointer.Draw(e.SpriteBatch);
        }
    }

    private void OnSearchChanged(SearchChangedEventArgs e) => this.ReinitializePointers();

    private void OnWarped(WarpedEventArgs e) => this.ReinitializePointers();

    private void OpenSearchBar() =>
        Game1.activeClickableMenu = new SearchOverlay(
            this.inputHelper,
            () => this.searchText.Value,
            value =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Log.Trace("{0}: Searching for {1}", this.Id, value);
                }

                if (this.searchText.Value == value)
                {
                    return;
                }

                this.searchText.Value = value;
                this.searchExpression.Value = this.expressionHandler.TryParseExpression(value, out var expression)
                    ? expression
                    : null;

                this.Events.Publish(new SearchChangedEventArgs(value, expression));
            });

    private bool Predicate(IStorageContainer container) =>
        container is not FarmerContainer
        && container.ChestFinder is FeatureOption.Enabled
        && (this.searchExpression.Value is null
            || this.searchExpression.Value.Equals(container.ToString())
            || this.searchExpression.Value.Equals(container.Items));

    private void ReinitializePointers()
    {
        this.pointers.Value.Clear();
        if (this.searchExpression.Value is null)
        {
            return;
        }

        if (this.menuHandler.CurrentMenu is not SearchOverlay && (!Game1.displayHUD || !Context.IsPlayerFree))
        {
            return;
        }

        var containers = this.containerFactory.GetAll(Game1.player.currentLocation, this.Predicate);
        this.pointers.Value.AddRange(containers.Select(container => new Pointer(container)));
        Log.Info("{0}: Found {1} chests", this.Id, this.pointers.Value.Count);
        this.currentIndex.Value = 0;
    }
}