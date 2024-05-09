namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Manages the menu by adding, removing, and filtering items.</summary>
internal sealed class MenuManager : BaseService
{
    private readonly ClickableTextureComponent downArrow;
    private readonly IEventManager eventManager;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly WeakReference<IClickableMenu?> source = new(null);
    private readonly ClickableTextureComponent upArrow;

    private int maxScroll;
    private int scrolled;

    /// <summary>Initializes a new instance of the <see cref="MenuManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public MenuManager(
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

    /// <summary>Gets or sets the method used to highlight an item in the inventory menu.</summary>
    public InventoryMenu.highlightThisItem? OriginalHighlightMethod { get; set; }

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
        if (parent?.allClickableComponents is null || current is not InventoryMenu inventoryMenu)
        {
            this.scrolled = 0;
            this.maxScroll = 0;
            return;
        }

        // Add arrows
        parent.allClickableComponents.Add(this.upArrow);
        parent.allClickableComponents.Add(this.downArrow);

        // Reposition arrows
        var topSlot = this.Columns - 1;
        var bottomSlot = this.Capacity - 1;
        this.upArrow.bounds.X = inventoryMenu.xPositionOnScreen + current.width + 8;
        this.upArrow.bounds.Y = inventoryMenu.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
        this.downArrow.bounds.X = inventoryMenu.xPositionOnScreen + current.width + 8;
        this.downArrow.bounds.Y = inventoryMenu.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

        // Assign Neighbor Ids
        this.upArrow.leftNeighborID = inventoryMenu.inventory[topSlot].myID;
        inventoryMenu.inventory[topSlot].rightNeighborID = this.upArrow.myID;
        this.downArrow.leftNeighborID = inventoryMenu.inventory[bottomSlot].myID;
        inventoryMenu.inventory[bottomSlot].rightNeighborID = this.downArrow.myID;
        this.upArrow.downNeighborID = this.downArrow.myID;
        this.downArrow.upNeighborID = this.upArrow.myID;
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
        if (this.InventoryMenu?.isWithinBounds(mouseX, mouseY) != true)
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
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.InventoryMenu?.isWithinBounds(mouseX, mouseY) != true)
        {
            return;
        }

        var scroll = this.modConfig.Controls.ScrollPage.IsDown() ? this.Rows : 1;
        this.scrolled += e.Delta > 0 ? -scroll : scroll;
    }
}