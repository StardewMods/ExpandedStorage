namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Adds a search bar to the top of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class SearchItems : BaseFeature<SearchItems>
{
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<SearchBar> searchBar;
    private readonly PerScreen<ISearchExpression?> searchExpression;

    /// <summary>Initializes a new instance of the <see cref="SearchItems" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchExpression">Dependency for retrieving a parsed search expression.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    /// <param name="searchText">Dependency for retrieving the unified search text.</param>
    public SearchItems(
        IEventManager eventManager,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        PerScreen<ISearchExpression?> searchExpression,
        SearchHandler searchHandler,
        PerScreen<string> searchText)
        : base(eventManager, log, manifest, modConfig)
    {
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.searchExpression = searchExpression;
        this.searchBar = new PerScreen<SearchBar>(
            () => new SearchBar(
                () => searchText.Value,
                value =>
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        this.Log.Trace("{0}: Searching for {1}", this.Id, value);
                    }

                    if (searchText.Value == value)
                    {
                        return;
                    }

                    searchText.Value = value;
                    this.searchExpression.Value = searchHandler.TryParseExpression(value, out var expression)
                        ? expression
                        : null;

                    this.Events.Publish(new SearchChangedEventArgs(this.searchExpression.Value));
                }));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SearchItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || this.itemGrabMenuManager.CurrentMenu is null
            || !this.itemGrabMenuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft:
                this.searchBar.Value.LeftClick(mouseX, mouseY);
                break;
            case SButton.MouseRight:
                this.searchBar.Value.RightClick(mouseX, mouseY);
                break;
            case SButton.Escape when this.itemGrabMenuManager.CurrentMenu.readyToClose():
                Game1.playSound("bigDeSelect");
                this.itemGrabMenuManager.CurrentMenu.exitThisMenu();
                this.inputHelper.Suppress(e.Button);
                return;
            case SButton.Escape: return;
        }

        if (this.searchBar.Value.Selected)
        {
            this.inputHelper.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.SearchItems != FeatureOption.Enabled
            || !this.Config.Controls.ToggleSearch.JustPressed())
        {
            return;
        }

        this.isActive.Value = !this.isActive.Value;
        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleSearch);
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        this.searchBar.Value.Draw(e.SpriteBatch);
    }

    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.searchBar.Value.Update(mouseX, mouseY);
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Menu is null
            || this.itemGrabMenuManager.Top.Container?.Options.SearchItems != FeatureOption.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        var top = this.itemGrabMenuManager.Top;
        this.searchBar.Value.Reset();
        this.searchBar.Value.Width = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);

        this.searchBar.Value.X = top.Columns switch
        {
            3 => top.Menu.inventory[1].bounds.Center.X - (this.searchBar.Value.Width / 2),
            12 => top.Menu.inventory[5].bounds.Right - (this.searchBar.Value.Width / 2),
            14 => top.Menu.inventory[6].bounds.Right - (this.searchBar.Value.Width / 2),
        };

        this.searchBar.Value.Y = top.Menu.yPositionOnScreen
            - (IClickableMenu.borderWidth / 2)
            - Game1.tileSize
            - (top.Rows == 3 ? 20 : 4);

        this.isActive.Value = true;
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (this.searchExpression.Value?.PartialMatch(e.Item) == false)
        {
            e.UnHighlight();
        }
    }

    private void OnItemsDisplaying(ItemsDisplayingEventArgs e)
    {
        if (this.searchExpression.Value is null
            || this.Config.SearchItemsMethod is not (FilterMethod.Sorted
                or FilterMethod.GrayedOut
                or FilterMethod.Hidden))
        {
            return;
        }

        e.Edit(
            items => this.Config.SearchItemsMethod switch
            {
                FilterMethod.Sorted or FilterMethod.GrayedOut => items.OrderByDescending(
                    this.searchExpression.Value.PartialMatch),
                FilterMethod.Hidden => items.Where(this.searchExpression.Value.PartialMatch),
                _ => items,
            });
    }

    private void OnSearchChanged(SearchChangedEventArgs e) => this.searchBar.Value.Reset();
}