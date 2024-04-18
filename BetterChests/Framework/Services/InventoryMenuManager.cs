namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

// TODO: Method for accessing InventoryMenu from GameMenu

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IInventoryMenuManager" />
internal sealed class InventoryMenuManager : BaseService, IInventoryMenuManager
{
    private readonly ClickableTextureComponent downArrow;
    private readonly IEventManager eventManager;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly WeakReference<InventoryMenu?> source = new(null);
    private readonly ClickableTextureComponent upArrow;

    private int maxScroll;
    private int scrolled;

    /// <summary>Initializes a new instance of the <see cref="InventoryMenuManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public InventoryMenuManager(
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(log, manifest)
    {
        // Init
        this.eventManager = eventManager;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;

        this.upArrow = new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom) { myID = 5318009 };

        this.downArrow = new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom) { myID = 5318008 };

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        eventManager.Subscribe<MouseWheelScrolledEventArgs>(this.OnMouseWheelScrolled);
        this.eventManager.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
    }

    /// <summary>Gets or sets the method used to highlight an item in the inventory menu.</summary>
    public InventoryMenu.highlightThisItem OriginalHighlightMethod { get; set; } = InventoryMenu.highlightAllItems;

    /// <inheritdoc />
    public InventoryMenu? Menu => this.source.TryGetTarget(out var target) ? target : null;

    /// <inheritdoc />
    public int Capacity => this.Menu?.capacity switch { null => 36, > 70 => 70, _ => this.Menu.capacity };

    /// <inheritdoc />
    public int Rows => this.Menu?.rows ?? 3;

    /// <inheritdoc />
    public int Columns => this.Capacity / this.Rows;

    /// <inheritdoc />
    public IStorageContainer? Container { get; set; }

    /// <summary>Draws overlay components to the SpriteBatch.</summary>
    /// <param name="spriteBatch">The SpriteBatch used to draw the game object.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);

        this.upArrow.scale = this.upArrow.containsPoint(mouseX, mouseY)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.upArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.upArrow.scale - 0.05f);

        this.downArrow.scale = this.downArrow.containsPoint(mouseX, mouseY)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.downArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.downArrow.scale - 0.05f);

        if (this.scrolled > 0)
        {
            this.upArrow.draw(spriteBatch);
        }

        if (this.scrolled < this.maxScroll)
        {
            this.downArrow.draw(spriteBatch);
        }
    }

    /// <summary>
    /// Resets the state of the menu, clearing all operations, cached items, and setting new arrow positions and
    /// neighbor IDs.
    /// </summary>
    /// <param name="parent">The parent ItemGrabMenu, if any.</param>
    /// <param name="current">The current InventoryMenu, if any.</param>
    public void Reset(ItemGrabMenu? parent, InventoryMenu? current)
    {
        this.source.SetTarget(current);
        if (parent is null || current is null)
        {
            this.scrolled = 0;
            this.maxScroll = 0;
            this.Container = null;
            return;
        }

        // Add arrows
        parent.allClickableComponents.Add(this.upArrow);
        parent.allClickableComponents.Add(this.downArrow);

        // Reposition arrows
        var topSlot = this.Columns - 1;
        var bottomSlot = this.Capacity - 1;
        this.upArrow.bounds.X = current.xPositionOnScreen + current.width + 8;
        this.upArrow.bounds.Y = current.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
        this.downArrow.bounds.X = current.xPositionOnScreen + current.width + 8;
        this.downArrow.bounds.Y = current.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

        // Assign Neighbor Ids
        this.upArrow.leftNeighborID = current.inventory[topSlot].myID;
        current.inventory[topSlot].rightNeighborID = this.upArrow.myID;
        this.downArrow.leftNeighborID = current.inventory[bottomSlot].myID;
        current.inventory[bottomSlot].rightNeighborID = this.downArrow.myID;
        this.upArrow.downNeighborID = this.downArrow.myID;
        this.downArrow.upNeighborID = this.upArrow.myID;
    }

    /// <summary>Highlights an item using the provided highlight methods.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>true if the item is successfully highlighted; otherwise, false.</returns>
    public bool HighlightMethod(Item item)
    {
        var original = this.OriginalHighlightMethod(item);
        if (!original || this.Container is null)
        {
            return original;
        }

        var itemHighlightingEventArgs = new ItemHighlightingEventArgs(this.Container, item);
        this.eventManager.Publish(itemHighlightingEventArgs);
        return itemHighlightingEventArgs.IsHighlighted;
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.scrolled > 0 && this.upArrow.containsPoint(mouseX, mouseY))
        {
            this.scrolled--;
            this.inputHelper.Suppress(e.Button);
        }

        if (this.scrolled < this.maxScroll && this.downArrow.containsPoint(mouseX, mouseY))
        {
            this.scrolled++;
            this.inputHelper.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.Menu?.isWithinBounds(mouseX, mouseY) != true)
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

    private void OnMouseWheelScrolled(MouseWheelScrolledEventArgs e)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.Menu?.isWithinBounds(mouseX, mouseY) != true)
        {
            return;
        }

        var scroll = this.modConfig.Controls.ScrollPage.IsDown() ? this.Rows : 1;
        this.scrolled += e.Delta > 0 ? -scroll : scroll;
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
}