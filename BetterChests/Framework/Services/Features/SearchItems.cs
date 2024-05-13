namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewValley.Menus;

/// <summary>Adds a search bar to the top of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class SearchItems : BaseFeature<SearchItems>
{
    private readonly AssetHandler assetHandler;
    private readonly PerScreen<ClickableTextureComponent?> existingStacksButton = new();
    private readonly IExpressionHandler expressionHandler;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new(() => true);
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<ClickableTextureComponent?> rejectButton = new();
    private readonly PerScreen<ClickableTextureComponent?> saveButton = new();
    private readonly PerScreen<TextField?> searchBar = new();
    private readonly PerScreen<IExpression?> searchExpression = new();
    private readonly PerScreen<string> searchText = new(() => string.Empty);

    /// <summary>Initializes a new instance of the <see cref="SearchItems" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    public SearchItems(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.assetHandler = assetHandler;
        this.expressionHandler = expressionHandler;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
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
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
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
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        if (container is null
            || !this.isActive.Value
            || this.searchBar.Value is null
            || this.menuHandler.CurrentMenu is not ItemGrabMenu
            || !this.menuHandler.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                if (this.searchBar.Value.LeftClick(mouseX, mouseY))
                {
                    this.inputHelper.Suppress(e.Button);
                    return;
                }

                if (container.CategorizeChest != FeatureOption.Enabled)
                {
                    return;
                }

                if (this.rejectButton.Value?.containsPoint(mouseX, mouseY) == true)
                {
                    this.inputHelper.Suppress(e.Button);
                    container.CategorizeChestBlockItems = container.CategorizeChestBlockItems == FeatureOption.Enabled
                        ? FeatureOption.Disabled
                        : FeatureOption.Enabled;

                    return;
                }

                if (this.saveButton.Value?.containsPoint(mouseX, mouseY) == true)
                {
                    this.inputHelper.Suppress(e.Button);
                    container.CategorizeChestSearchTerm = this.searchText.Value;
                    return;
                }

                if (this.existingStacksButton.Value?.containsPoint(mouseX, mouseY) == true)
                {
                    this.inputHelper.Suppress(e.Button);
                    container.CategorizeChestIncludeStacks =
                        container.CategorizeChestIncludeStacks == FeatureOption.Enabled
                            ? FeatureOption.Disabled
                            : FeatureOption.Enabled;

                    this.existingStacksButton.Value.sourceRect =
                        container.CategorizeChestIncludeStacks == FeatureOption.Enabled
                            ? new Rectangle(236, 425, 9, 9)
                            : new Rectangle(227, 425, 9, 9);
                }

                break;

            case SButton.MouseRight or SButton.ControllerB:
                if (this.searchBar.Value.RightClick(mouseX, mouseY))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                break;

            case SButton.Escape when this.menuHandler.CurrentMenu.readyToClose():
                this.inputHelper.Suppress(e.Button);
                Game1.playSound("bigDeSelect");
                this.menuHandler.CurrentMenu.exitThisMenu();
                break;

            case SButton.Escape: return;

            default:
                if (this.searchBar.Value.Selected
                    && e.Button is not (SButton.LeftShift
                        or SButton.RightShift
                        or SButton.LeftAlt
                        or SButton.RightAlt
                        or SButton.LeftControl
                        or SButton.RightControl))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (this.menuHandler.Top.Container?.SearchItems is not FeatureOption.Enabled)
        {
            return;
        }

        // Toggle Search Bar
        if (this.Config.Controls.ToggleSearch.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleSearch);
            this.isActive.Value = !this.isActive.Value;
            return;
        }

        // Copy Search
        if (this.searchBar.Value?.Selected == true && this.Config.Controls.Copy.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.Copy);
            DesktopClipboard.SetText(this.searchText.Value);
            return;
        }

        // Paste Search
        if (this.searchBar.Value?.Selected == true && this.Config.Controls.Paste.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.Paste);
            var pasteText = string.Empty;
            DesktopClipboard.GetText(ref pasteText);
            this.searchText.Value = pasteText;
            this.searchBar.Value.Reset();
            return;
        }

        // Clear Search
        if (this.isActive.Value && this.Config.Controls.ClearSearch.JustPressed())
        {
            this.searchText.Value = string.Empty;
            this.searchExpression.Value = null;
            this.Events.Publish(new SearchChangedEventArgs(string.Empty, null));
        }
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        var top = this.menuHandler.Top;
        if (top.InventoryMenu is null || container?.SearchItems is not FeatureOption.Enabled)
        {
            this.searchBar.Value = null;
            return;
        }

        var width = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);

        var x = top.Columns switch
        {
            3 => top.InventoryMenu.inventory[1].bounds.Center.X - (width / 2),
            12 => top.InventoryMenu.inventory[5].bounds.Right - (width / 2),
            14 => top.InventoryMenu.inventory[6].bounds.Right - (width / 2),
            _ => (Game1.uiViewport.Width - width) / 2,
        };

        var y = top.InventoryMenu.yPositionOnScreen
            - (IClickableMenu.borderWidth / 2)
            - Game1.tileSize
            - (top.Rows == 3 ? 25 : 4);

        this.searchBar.Value = new TextField(
            x,
            y,
            width,
            () => this.searchText.Value,
            value =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.Log.Trace("{0}: Searching for {1}", this.Id, value);
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

        if (container.CategorizeChest != FeatureOption.Enabled)
        {
            this.saveButton.Value = null;
            this.existingStacksButton.Value = null;
            this.rejectButton.Value = null;
            return;
        }

        x += width;
        y += 8;

        if (this.assetHandler.Icons.TryGetValue(this.ModId + "/Save", out var icon))
        {
            this.saveButton.Value = new ClickableTextureComponent(
                new Rectangle(x, y, Game1.tileSize / 2, Game1.tileSize / 2),
                this.assetHandler.UiTexture,
                icon.Area,
                2f)
            {
                name = "Save",
                hoverText = I18n.Button_SaveAsCategorization_Name(),
                myID = 8_675_309,
            };
        }

        this.existingStacksButton.Value = new ClickableTextureComponent(
            new Rectangle(x + 32, y + 2, 27, 27),
            Game1.mouseCursors,
            container.CategorizeChestIncludeStacks == FeatureOption.Enabled
                ? new Rectangle(236, 425, 9, 9)
                : new Rectangle(227, 425, 9, 9),
            3f)
        {
            name = "ExistingStacks",
            hoverText = I18n.Button_IncludeExistingStacks_Name(),
            myID = 8_675_310,
        };

        this.rejectButton.Value = new ClickableTextureComponent(
            new Rectangle(x + 64, y + 2, 24, 24),
            Game1.mouseCursors,
            new Rectangle(322, 498, 12, 12),
            2f)
        {
            name = "Reject",
            hoverText = I18n.Button_RejectUncategorized_Name(),
            myID = 8_675_308,
        };
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (e.Container.SearchItems is FeatureOption.Enabled && this.searchExpression.Value?.Equals(e.Item) == false)
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
                    this.searchExpression.Value.Equals),
                FilterMethod.Hidden => items.Where(this.searchExpression.Value.Equals),
                _ => items,
            });
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        if (this.searchBar.Value is null
            || !this.isActive.Value
            || this.menuHandler.CurrentMenu is not ItemGrabMenu itemGrabMenu
            || container is null)
        {
            return;
        }

        this.searchBar.Value.Draw(e.SpriteBatch);

        if (container.CategorizeChest != FeatureOption.Enabled)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);

        if (this.saveButton.Value is not null)
        {
            this.saveButton.Value.tryHover(mouseX, mouseY);
            this.saveButton.Value.draw(e.SpriteBatch);
            if (this.saveButton.Value.containsPoint(mouseX, mouseY))
            {
                itemGrabMenu.hoverText = this.saveButton.Value.hoverText;
                return;
            }
        }

        if (this.existingStacksButton.Value is not null)
        {
            this.existingStacksButton.Value.tryHover(mouseX, mouseY);
            this.existingStacksButton.Value.draw(e.SpriteBatch);
            if (this.existingStacksButton.Value.containsPoint(mouseX, mouseY))
            {
                itemGrabMenu.hoverText = this.existingStacksButton.Value.hoverText;
                return;
            }
        }

        if (this.rejectButton.Value is not null)
        {
            this.rejectButton.Value.tryHover(mouseX, mouseY);
            this.rejectButton.Value.draw(
                e.SpriteBatch,
                container.CategorizeChestBlockItems == FeatureOption.Enabled ? Color.White : Color.Gray,
                1f);

            if (this.rejectButton.Value.containsPoint(mouseX, mouseY))
            {
                itemGrabMenu.hoverText = this.rejectButton.Value.hoverText;
            }
        }
    }

    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (this.searchBar.Value is null || !this.isActive.Value || this.menuHandler.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.searchBar.Value.Update(mouseX, mouseY);
    }

    private void OnSearchChanged(SearchChangedEventArgs e)
    {
        if (this.searchBar.Value is null || !this.isActive.Value || this.menuHandler.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        this.searchBar.Value.Reset();
    }
}