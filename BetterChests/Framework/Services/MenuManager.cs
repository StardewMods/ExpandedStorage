namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Manages the menu by adding, removing, and filtering items.</summary>
internal sealed class MenuManager
{
    private readonly IEventManager eventManager;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly MenuHandler menuHandler;
    private readonly IModConfig modConfig;
    private readonly WeakReference<IClickableMenu?> source = new(null);

    private ClickableTextureComponent? downArrow;
    private int maxScroll;
    private int scrolled;
    private ClickableTextureComponent? upArrow;

    /// <summary>Initializes a new instance of the <see cref="MenuManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public MenuManager(
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        IModConfig modConfig)
    {
        // Init
        this.eventManager = eventManager;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.modConfig = modConfig;

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        eventManager.Subscribe<MouseWheelScrolledEventArgs>(this.OnMouseWheelScrolled);
        eventManager.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
    }

    /// <summary>Gets the capacity of the inventory menu.</summary>
    public int Capacity =>
        this.InventoryMenu?.capacity switch { null => 36, > 70 => 70, _ => this.InventoryMenu?.capacity ?? 0 };

    /// <summary>Gets the number of columns of the inventory menu.</summary>
    public int Columns => this.Capacity / this.Rows;

    /// <summary>Gets the instance of the inventory menu that is being managed.</summary>
    public InventoryMenu? InventoryMenu => this.Menu as InventoryMenu;

    /// <summary>Gets the instance of the menu that is being managed.</summary>
    public IClickableMenu? Menu => this.source.TryGetTarget(out var target) ? target : null;

    /// <summary>Gets the number of rows of the inventory menu.</summary>
    public int Rows => this.InventoryMenu?.rows ?? 3;

    /// <summary>Gets or sets the container associated with the menu.</summary>
    public IStorageContainer? Container { get; set; }

    /// <summary>Gets the inventory icon.</summary>
    public ClickableComponent? Icon { get; private set; }

    /// <summary>Gets or sets the method used to highlight an item in the inventory menu.</summary>
    public InventoryMenu.highlightThisItem? OriginalHighlightMethod { get; set; }

    private ClickableTextureComponent DownArrow =>
        this.downArrow ??= this.iconRegistry.Icon(VanillaIcon.ArrowDown).Component(IconStyle.Transparent);

    private ClickableTextureComponent UpArrow =>
        this.upArrow ??= this.iconRegistry.Icon(VanillaIcon.ArrowUp).Component(IconStyle.Transparent);

    /// <summary>Draws overlay components to the SpriteBatch.</summary>
    /// <param name="spriteBatch">The SpriteBatch used to draw the game object.</param>
    /// <param name="cursor">The mouse position.</param>
    public void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        this.UpArrow.scale = this.UpArrow.bounds.Contains(cursor)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.UpArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.UpArrow.scale - 0.05f);

        this.DownArrow.scale = this.DownArrow.bounds.Contains(cursor)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.DownArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.DownArrow.scale - 0.05f);

        if (this.scrolled > 0)
        {
            this.UpArrow.draw(spriteBatch);
        }

        if (this.scrolled < this.maxScroll)
        {
            this.DownArrow.draw(spriteBatch);
        }

        // Draw storage icon
        if (this.Icon is not ClickableTextureComponent clickableTextureComponent)
        {
            return;
        }

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Icon.bounds.X, this.Icon.bounds.Y + 44),
            new Rectangle(21, 368, 11, 16),
            Color.White,
            (float)Math.PI * 3f / 2f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);

        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Icon.bounds.X, this.Icon.bounds.Y + Game1.tileSize + 12),
            new Rectangle(16, 368, 12, 16),
            Color.White,
            (float)Math.PI * 3f / 2f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);

        spriteBatch.Draw(
            clickableTextureComponent.texture,
            new Vector2(
                this.Icon.bounds.Center.X - (clickableTextureComponent.sourceRect.Width * Game1.pixelZoom / 2f) + 8,
                this.Icon.bounds.Center.Y - (clickableTextureComponent.sourceRect.Height * Game1.pixelZoom / 2f)),
            clickableTextureComponent.sourceRect,
            Color.White,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);
    }

    /// <summary>Highlights an item using the provided highlight methods.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns><c>true</c> if the item is successfully highlighted; otherwise, <c>false</c>.</returns>
    public bool HighlightMethod(Item item)
    {
        var original = this.OriginalHighlightMethod is null || this.OriginalHighlightMethod(item);
        if (!original || this.Container is null)
        {
            return original;
        }

        var itemHighlightingEventArgs = new ItemHighlightingEventArgs(this.Container, item);
        this.eventManager.Publish(itemHighlightingEventArgs);
        return itemHighlightingEventArgs.IsHighlighted;
    }

    /// <summary>Set the menu, clearing all operations, cached items, and set new arrow positions and neighbor IDs.</summary>
    /// <param name="parent">The parent menu, if any.</param>
    /// <param name="current">The current menu, if any.</param>
    public void Set(IClickableMenu? parent, IClickableMenu? current)
    {
        this.source.SetTarget(current);
        this.Icon = null;
        if (parent is null)
        {
            this.scrolled = 0;
            this.maxScroll = 0;
            return;
        }

        // Add arrows
        parent.allClickableComponents?.Add(this.UpArrow);
        parent.allClickableComponents?.Add(this.DownArrow);

        if (this.InventoryMenu is not null)
        {
            // Reposition arrows
            var topSlot = this.Columns - 1;
            var bottomSlot = this.Capacity - 1;
            this.UpArrow.bounds.X = this.InventoryMenu.xPositionOnScreen + this.InventoryMenu.width + 8;
            this.UpArrow.bounds.Y = this.InventoryMenu.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
            this.DownArrow.bounds.X = this.InventoryMenu.xPositionOnScreen + this.InventoryMenu.width + 8;
            this.DownArrow.bounds.Y = this.InventoryMenu.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

            // Assign Neighbor Ids
            this.UpArrow.leftNeighborID = this.InventoryMenu.inventory[topSlot].myID;
            this.InventoryMenu.inventory[topSlot].rightNeighborID = this.UpArrow.myID;
            this.DownArrow.leftNeighborID = this.InventoryMenu.inventory[bottomSlot].myID;
            this.InventoryMenu.inventory[bottomSlot].rightNeighborID = this.DownArrow.myID;
            this.UpArrow.downNeighborID = this.DownArrow.myID;
            this.DownArrow.upNeighborID = this.UpArrow.myID;
        }

        // Add icon
        int x;
        int y;
        switch (this.menuHandler.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu when this == this.menuHandler.Top && this.InventoryMenu is not null:
                x = this.InventoryMenu.xPositionOnScreen - Game1.tileSize - 36;
                y = itemGrabMenu.yPositionOnScreen + 4;
                break;
            case ItemGrabMenu itemGrabMenu when this == this.menuHandler.Bottom:
                x = itemGrabMenu.xPositionOnScreen - Game1.tileSize;
                y = itemGrabMenu.yPositionOnScreen + (int)(itemGrabMenu.height / 2f) + 4;
                break;
            case InventoryPage when this.InventoryMenu is not null:
                x = this.InventoryMenu.xPositionOnScreen - Game1.tileSize - 36;
                y = this.InventoryMenu.yPositionOnScreen + 24;
                break;
            case ShopMenu shopMenu when this == this.menuHandler.Top && !shopMenu.tabButtons.Any():
                x = shopMenu.xPositionOnScreen - Game1.tileSize + 4;
                y = shopMenu.yPositionOnScreen + Game1.tileSize + 24;
                break;
            case ShopMenu when this == this.menuHandler.Bottom && this.InventoryMenu is not null:
                x = this.InventoryMenu.xPositionOnScreen - Game1.tileSize - 20;
                y = this.InventoryMenu.yPositionOnScreen + 24;
                break;
            default: return;
        }

        if (string.IsNullOrWhiteSpace(this.Container?.StorageIcon)
            || !this.iconRegistry.TryGetIcon(this.Container.StorageIcon, out var storageIcon))
        {
            this.Icon = new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize + 12), "icon");
            return;
        }

        this.Icon = storageIcon.Component(IconStyle.Transparent, x, y);
        this.Icon.bounds.Size = new Point(Game1.tileSize, Game1.tileSize + 12);
        parent.allClickableComponents?.Add(this.Icon);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft)
        {
            return;
        }

        var cursor = e.Cursor.GetScaledScreenPixels();
        if (this.scrolled > 0 && this.UpArrow.bounds.Contains(cursor))
        {
            this.scrolled--;
            this.inputHelper.Suppress(e.Button);
        }

        if (this.scrolled < this.maxScroll && this.DownArrow.bounds.Contains(cursor))
        {
            this.scrolled++;
            this.inputHelper.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        var cursor = e.Cursor.GetScaledScreenPixels().ToPoint();
        if (this.InventoryMenu?.isWithinBounds(cursor.X, cursor.Y) != true)
        {
            return;
        }

        if (this.modConfig.Controls.ScrollUp.JustPressed())
        {
            this.scrolled--;
            this.inputHelper.SuppressActiveKeybinds(this.modConfig.Controls.ScrollUp);
        }

        if (this.modConfig.Controls.ScrollDown.JustPressed())
        {
            this.scrolled++;
            this.inputHelper.SuppressActiveKeybinds(this.modConfig.Controls.ScrollDown);
        }
    }

    [Priority(int.MinValue)]
    private void OnItemsDisplaying(ItemsDisplayingEventArgs e)
    {
        if (e.Container != this.Container)
        {
            return;
        }

        var totalRows = (int)Math.Ceiling((double)e.Items.Count() / this.Columns);
        this.maxScroll = Math.Max(0, totalRows - this.Rows);
        this.scrolled = Math.Max(0, Math.Min(this.scrolled, this.maxScroll));

        if (this.scrolled == 0 && this.maxScroll == 0)
        {
            return;
        }

        e.Edit(items => items.Skip(this.scrolled * this.Columns).Take(this.Capacity));
    }

    private void OnMouseWheelScrolled(MouseWheelScrolledEventArgs e)
    {
        var cursor = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        if (this.InventoryMenu?.isWithinBounds(cursor.X, cursor.Y) != true)
        {
            return;
        }

        var scroll = this.modConfig.Controls.ScrollPage.IsDown() ? this.Rows : 1;
        this.scrolled += e.Delta > 0 ? -scroll : scroll;
    }
}