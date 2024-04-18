namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

/// <summary>Access chests remotely.</summary>
internal sealed class AccessChest : BaseFeature<AccessChest>
{
    private readonly PerScreen<Rectangle> bounds = new(() => Rectangle.Empty);
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<SortedList<string, IStorageContainer>> currentContainers = new(() => []);
    private readonly PerScreen<ClickableComponent?> dropDown = new();
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<List<ClickableComponent>> items = new(() => []);
    private readonly PerScreen<ClickableTextureComponent> leftArrow;
    private readonly PerScreen<int> offset = new();
    private readonly PerScreen<ClickableTextureComponent> rightArrow;
    private readonly PerScreen<ISearchExpression?> searchExpression;

    /// <summary>Initializes a new instance of the <see cref="AccessChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchExpression">Dependency for retrieving a parsed search expression.</param>
    public AccessChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        PerScreen<ISearchExpression?> searchExpression)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.searchExpression = searchExpression;

        this.leftArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
                Game1.mouseCursors,
                new Rectangle(352, 495, 11, 12),
                Game1.pixelZoom) { myID = 5318007 });

        this.rightArrow = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
                Game1.mouseCursors,
                new Rectangle(365, 495, 11, 12),
                Game1.pixelZoom) { myID = 5318006 });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.AccessChest != RangeOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<MouseWheelScrolledEventArgs>(this.OnMouseWheelScrolled);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<MouseWheelScrolledEventArgs>(this.OnMouseWheelScrolled);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    private bool Predicate(IStorageContainer container) =>
        container is not FarmerContainer
        && (this.searchExpression.Value is null || container.Items.Any(this.searchExpression.Value.PartialMatch))
        && container.Options.AccessChest is not (RangeOption.Disabled or RangeOption.Default)
        && container.Options.AccessChest.WithinRange(-1, container.Location, container.TileLocation);

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (this.dropDown.Value is null
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || !this.itemGrabMenuManager.TryGetFocus(this, out var focus))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.dropDown.Value.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            this.isActive.Value = !this.isActive.Value;
            if (this.isActive.Value)
            {
                if (this.itemGrabMenuManager.Top.Container is not null)
                {
                    var currentIndex =
                        this.currentContainers.Value.IndexOfValue(this.itemGrabMenuManager.Top.Container);

                    this.offset.Value = Math.Clamp(
                        currentIndex,
                        0,
                        Math.Max(0, this.currentContainers.Value.Count - 10));
                }
                else
                {
                    this.offset.Value = 0;
                }
            }
            else
            {
                focus.Release();
            }

            return;
        }

        focus.Release();
        if (this.itemGrabMenuManager.Top.Container is null) { }
        else if (this.leftArrow.Value.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            this.isActive.Value = !this.isActive.Value;
            var previousIndex = this.currentContainers.Value.IndexOfValue(this.itemGrabMenuManager.Top.Container) - 1;
            if (previousIndex < 0)
            {
                previousIndex = this.currentContainers.Value.Count - 1;
            }

            var previousContainer = this.currentContainers.Value.Values.ElementAtOrDefault(previousIndex);
            previousContainer?.ShowMenu();
            return;
        }
        else if (this.rightArrow.Value.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            this.isActive.Value = !this.isActive.Value;
            var nextIndex = this.currentContainers.Value.IndexOfValue(this.itemGrabMenuManager.Top.Container) + 1;
            if (nextIndex >= this.currentContainers.Value.Count)
            {
                nextIndex = 0;
            }

            var nextContainer = this.currentContainers.Value.Values.ElementAtOrDefault(nextIndex);
            nextContainer?.ShowMenu();
            return;
        }

        if (!this.bounds.Value.Contains(mouseX, mouseY) || !this.isActive.Value)
        {
            this.isActive.Value = false;
            return;
        }

        this.inputHelper.Suppress(e.Button);
        var item = this.items.Value.FirstOrDefault(i => i.bounds.Contains(mouseX, mouseY));
        if (item is null)
        {
            return;
        }

        var selectedIndex = int.Parse(item.name, CultureInfo.InvariantCulture) + this.offset.Value;
        var selectedContainer = this.currentContainers.Value.Values.ElementAtOrDefault(selectedIndex);
        selectedContainer?.ShowMenu();
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (Context.IsPlayerFree && this.Config.Controls.AccessChests.JustPressed())
        {
            // Access First Chest
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.AccessChests);
            this.containerFactory.GetAll(this.Predicate).MinBy(c => c.ToString())?.ShowMenu();
            return;
        }

        if (this.dropDown.Value is null
            || this.itemGrabMenuManager.Top.Container is null
            || !this.itemGrabMenuManager.CanFocus(this))
        {
            return;
        }

        if (this.Config.Controls.AccessPreviousChest.JustPressed())
        {
            var previousIndex = this.currentContainers.Value.IndexOfValue(this.itemGrabMenuManager.Top.Container) - 1;
            if (previousIndex < 0)
            {
                previousIndex = this.currentContainers.Value.Count - 1;
            }

            var nextContainer = this.currentContainers.Value.Values.ElementAtOrDefault(previousIndex);
            nextContainer?.ShowMenu();
            return;
        }

        if (this.Config.Controls.AccessNextChest.JustPressed())
        {
            var nextIndex = this.currentContainers.Value.IndexOfValue(this.itemGrabMenuManager.Top.Container) + 1;
            if (nextIndex >= this.currentContainers.Value.Count)
            {
                nextIndex = 0;
            }

            var nextContainer = this.currentContainers.Value.Values.ElementAtOrDefault(nextIndex);
            nextContainer?.ShowMenu();
        }
    }

    private void OnMouseWheelScrolled(MouseWheelScrolledEventArgs e)
    {
        if (this.dropDown.Value is null || !this.isActive.Value)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.bounds.Value.Contains(mouseX, mouseY))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.offset.Value--;
                break;
            case < 0:
                this.offset.Value++;
                break;
            default: return;
        }

        this.offset.Value = Math.Clamp(this.offset.Value, 0, this.currentContainers.Value.Count - 10);
    }

    [Priority(int.MinValue + 1)]
    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (this.dropDown.Value is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);

        // Draw current container index
        if (this.itemGrabMenuManager.Top.Container is not null && this.Config.AccessChestsShowArrows)
        {
            var currentIndex = this.currentContainers.Value.IndexOfValue(this.itemGrabMenuManager.Top.Container);
            if (currentIndex != -1)
            {
                var textIndex = currentIndex.ToString(CultureInfo.InvariantCulture);
                var width = SpriteText.getWidthOfString(textIndex);
                SpriteText.drawString(
                    e.SpriteBatch,
                    textIndex,
                    (this.leftArrow.Value.bounds.Left + this.rightArrow.Value.bounds.Left + Game1.tileSize - width) / 2,
                    this.leftArrow.Value.bounds.Y,
                    999999,
                    -1,
                    999999,
                    1f,
                    1f,
                    false,
                    3,
                    string.Empty,
                    SpriteText.color_White);

                this.leftArrow.Value.scale = this.leftArrow.Value.containsPoint(mouseX, mouseY)
                    ? Math.Min(Game1.pixelZoom * 1.1f, this.leftArrow.Value.scale + 0.05f)
                    : Math.Max(Game1.pixelZoom, this.leftArrow.Value.scale - 0.05f);

                this.rightArrow.Value.scale = this.rightArrow.Value.containsPoint(mouseX, mouseY)
                    ? Math.Min(Game1.pixelZoom * 1.1f, this.rightArrow.Value.scale + 0.05f)
                    : Math.Max(Game1.pixelZoom, this.rightArrow.Value.scale - 0.05f);

                this.leftArrow.Value.draw(e.SpriteBatch);
                this.rightArrow.Value.draw(e.SpriteBatch);
            }
        }

        // Draw current container name
        IClickableMenu.drawHoverText(
            e.SpriteBatch,
            this.dropDown.Value.name,
            Game1.smallFont,
            overrideX: this.dropDown.Value.bounds.X,
            overrideY: this.dropDown.Value.bounds.Y);

        // Draw dropdown
        if (!this.isActive.Value)
        {
            return;
        }

        IClickableMenu.drawTextureBox(
            e.SpriteBatch,
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            this.bounds.Value.X,
            this.bounds.Value.Y,
            this.bounds.Value.Width,
            this.bounds.Value.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            0.97f);

        // Draw Values
        foreach (var item in this.items.Value)
        {
            var index = int.Parse(item.name, CultureInfo.InvariantCulture) + this.offset.Value;
            var value = this.currentContainers.Value.Values.ElementAtOrDefault(index);
            if (value is null)
            {
                continue;
            }

            if (item.bounds.Contains(mouseX, mouseY))
            {
                e.SpriteBatch.Draw(
                    Game1.staminaRect,
                    new Rectangle(item.bounds.X, item.bounds.Y, this.bounds.Value.Width - 16, item.bounds.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.Wheat,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.975f);
            }

            e.SpriteBatch.DrawString(
                Game1.smallFont,
                value.ToString(),
                new Vector2(item.bounds.X, item.bounds.Y),
                Game1.textColor);
        }

        // Draw up indicator
        if (this.offset.Value > 0)
        {
            e.SpriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(this.bounds.Value.X + 8, this.bounds.Value.Top + 8, this.bounds.Value.Width - 16, 4),
                Game1.textColor);
        }

        // Draw down indicator
        if (this.currentContainers.Value.Count - this.offset.Value > 11)
        {
            e.SpriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(this.bounds.Value.X + 8, this.bounds.Value.Bottom - 12, this.bounds.Value.Width - 16, 4),
                Game1.textColor);
        }
    }

    private void OnSearchChanged(SearchChangedEventArgs e)
    {
        if (this.dropDown.Value is not null)
        {
            this.ReinitializeContainers();
        }
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        this.isActive.Value = false;
        var top = this.itemGrabMenuManager.Top;
        if (this.itemGrabMenuManager.CurrentMenu is null || top.Container is null || top.Menu is null)
        {
            this.dropDown.Value = null;
            return;
        }

        var name = string.IsNullOrWhiteSpace(top.Container.Options.StorageName)
            ? top.Container.DisplayName
            : top.Container.Options.StorageName;

        var x = Math.Max(IClickableMenu.borderWidth / 2, (Game1.uiViewport.Width / 2) - (Game1.tileSize * 10));
        var y = IClickableMenu.borderWidth / 2;

        this.leftArrow.Value.bounds.X = x;
        this.leftArrow.Value.bounds.Y = y + Game1.tileSize + 20;

        this.rightArrow.Value.bounds.X = x + (Game1.tileSize * 2);
        this.rightArrow.Value.bounds.Y = y + Game1.tileSize + 20;

        var (width, height) = Game1.smallFont.MeasureString(name);
        this.dropDown.Value = new ClickableComponent(
            new Rectangle(x, y, (int)width + IClickableMenu.borderWidth, (int)height + IClickableMenu.borderWidth),
            name,
            top.Container.ToString());

        this.ReinitializeContainers();
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (this.isActive.Value)
        {
            e.UnHighlight();
        }
    }

    private void ReinitializeContainers()
    {
        var top = this.itemGrabMenuManager.Top;
        if (this.dropDown.Value is null || top.Container is null || top.Menu is null)
        {
            return;
        }

        var containers = this.containerFactory.GetAll(this.Predicate);
        this.currentContainers.Value.Clear();
        foreach (var container in containers)
        {
            this.currentContainers.Value.TryAdd(container.ToString()!, container);
        }

        var textValues = this.currentContainers.Value.Keys;
        var textBounds = textValues.Select(value => Game1.smallFont.MeasureString(value).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);
        this.bounds.Value = new Rectangle(
            this.dropDown.Value.bounds.X,
            this.dropDown.Value.bounds.Bottom,
            textBounds.Max(b => b.X) + 16,
            textBounds.Take(10).Sum(b => b.Y) + 32);

        this.items.Value = Enumerable
            .Range(0, 10)
            .Select(
                i => new ClickableComponent(
                    new Rectangle(
                        this.bounds.Value.X + 8,
                        this.bounds.Value.Y + 16 + (textHeight * i),
                        this.bounds.Value.Width,
                        textHeight),
                    i.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }
}