namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Terms;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Helpers;
using StardewMods.GarbageDay.Common.UI;
using StardewValley.Menus;

/// <summary>Visual menu for search bar.</summary>
internal sealed class SearchMenu : BaseMenu
{
    private readonly ClickableTextureComponent arrowDownLeft;
    private readonly ClickableTextureComponent arrowDownRight;
    private readonly ClickableTextureComponent arrowUpLeft;
    private readonly ClickableTextureComponent arrowUpRight;
    private readonly InventoryMenu inventory;
    private readonly ClickableTextureComponent scrollBar;
    private readonly Rectangle scrollBarRunner;
    private readonly SearchBar searchBar;
    private readonly SearchHandler searchHandler;

    private List<Item> allItems = [];
    private int rowOffset;
    private int scrollAmount;
    private int scrollArea;
    private bool scrolling;
    private ISearchExpression? searchExpression;
    private string searchText;
    private int totalRows;

    /// <summary>Initializes a new instance of the <see cref="SearchMenu" /> class.</summary>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    public SearchMenu(SearchHandler searchHandler)
    {
        var myId = 55378008;
        this.searchHandler = searchHandler;
        this.searchText = string.Empty;

        this.inventory = new InventoryMenu(
            this.xPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)

            //+ Game1.tileSize
            + 428,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            false,
            new List<Item>(),
            this.HighlightMethod,
            42,
            7);

        this.inventory.populateClickableComponentList();

        this.SearchText = string.Empty;
        this.searchBar = new SearchBar(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + Game1.tileSize,
            this.width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            () => this.SearchText,
            value => this.SearchText = value);

        var x1 =
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 348;

        var x2 = this.xPositionOnScreen
            + this.width
            - IClickableMenu.spaceToClearSideBorder

            //- (IClickableMenu.borderWidth / 2)
            //- Game1.tileSize
            + 12;

        var y1 = this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 16;

        var y2 = this.yPositionOnScreen
            + this.height
            - IClickableMenu.spaceToClearSideBorder
            - (IClickableMenu.borderWidth / 2)
            - Game1.tileSize;

        this.arrowUpLeft = new ClickableTextureComponent(
            new Rectangle(x1, y1, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom) { myID = ++myId };

        this.arrowDownLeft = new ClickableTextureComponent(
            new Rectangle(x1, y2, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom) { myID = ++myId };

        this.arrowUpRight = new ClickableTextureComponent(
            new Rectangle(x2, y1, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom) { myID = ++myId };

        this.arrowDownRight = new ClickableTextureComponent(
            new Rectangle(x2, y2, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom) { myID = ++myId };

        this.scrollBar = new ClickableTextureComponent(
            new Rectangle(
                this.arrowUpLeft.bounds.X + 12,
                this.arrowUpLeft.bounds.Y + this.arrowUpLeft.bounds.Height + 4,
                24,
                40),
            Game1.mouseCursors,
            new Rectangle(435, 463, 6, 10),
            Game1.pixelZoom);

        var scrollBarRunnerHeight = y2 - y1 - this.arrowUpLeft.bounds.Height - 12;

        this.scrollBarRunner = new Rectangle(
            this.scrollBar.bounds.X,
            this.arrowUpLeft.bounds.Y + this.arrowUpLeft.bounds.Height + 4,
            this.scrollBar.bounds.Width,
            scrollBarRunnerHeight);

        this.populateClickableComponentList();
    }

    private string SearchText
    {
        get => this.searchText;
        set
        {
            this.searchText = value;
            this.searchExpression = this.searchHandler.TryParseExpression(value, out var expression)
                ? expression
                : null;

            if (this.searchExpression is null)
            {
                this.allItems = [];
                this.inventory.actualInventory.Clear();
                this.totalRows = 0;
                return;
            }

            this.allItems = ItemRepository.GetItems(this.searchExpression.PartialMatch).ToList();
            this.inventory.actualInventory = this.allItems.Take(this.inventory.capacity).ToList();
            this.totalRows = (int)Math.Ceiling(
                (float)this.allItems.Count / (this.inventory.capacity / this.inventory.rows));
        }
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        base.draw(b, -1);

        this.drawHorizontalPartition(
            b,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.drawVerticalIntersectingPartition(
            b,
            this.xPositionOnScreen + (IClickableMenu.borderWidth / 2) + 400,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.searchBar.Draw(b);
        this.inventory.draw(b);

        // Scrollbar
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(403, 383, 6, 6),
            this.scrollBarRunner.X,
            this.scrollBarRunner.Y,
            this.scrollBarRunner.Width,
            this.scrollBarRunner.Height,
            Color.White,
            4f);

        this.scrollBar.draw(b);

        // Arrows
        this.arrowUpLeft.draw(b, this.scrollAmount > 0 ? Color.White : Color.Black * 0.35f, 1f);
        this.arrowDownLeft.draw(b, this.scrollAmount < this.totalRows - 1 ? Color.White : Color.Black * 0.35f, 1f);
        this.arrowUpRight.draw(b, this.rowOffset > 0 ? Color.White : Color.Black * 0.35f, 1f);
        this.arrowDownRight.draw(
            b,
            this.rowOffset < this.totalRows - this.inventory.rows - 1 ? Color.White : Color.Black * 0.35f,
            1f);

        if (this.searchExpression is null)
        {
            Game1.mouseCursorTransparency = 1f;
            this.drawMouse(b);
            return;
        }

        var baseX = this.xPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + 16;

        var currentX = baseX;

        var currentY = this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12;

        const int indent = 12;
        Enqueue(this.searchExpression);
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
        return;

        void Enqueue(ISearchExpression currentExpression)
        {
            switch (currentExpression)
            {
                case AnyExpression any:
                    if (any.Expressions.Length > 1)
                    {
                        b.DrawString(Game1.smallFont, "- [ANY]", new Vector2(currentX - 10, currentY), Game1.textColor);
                        currentY += Game1.smallFont.MeasureString("- [ANY]").ToPoint().Y;
                    }

                    currentX += indent;
                    foreach (var expression in any.Expressions) { Enqueue(expression); }

                    currentX -= indent;
                    break;

                case AllExpression all:
                    if (all.Expressions.Length > 1)
                    {
                        b.DrawString(Game1.smallFont, "- [ALL]", new Vector2(currentX - 10, currentY), Game1.textColor);
                        currentY += Game1.smallFont.MeasureString("- [ALL]").ToPoint().Y;
                    }

                    currentX += indent;
                    foreach (var expression in all.Expressions) { Enqueue(expression); }

                    currentX -= indent;
                    break;

                case NotExpression not:
                    b.DrawString(Game1.smallFont, "- [NOT]", new Vector2(currentX - 10, currentY), Game1.textColor);
                    currentY += Game1.smallFont.MeasureString("- [NOT]").ToPoint().Y;
                    currentX += indent;
                    Enqueue(not.InnerExpression);
                    currentX -= indent;
                    break;

                case SearchTerm searchTerm:
                    b.DrawString(
                        Game1.smallFont,
                        searchTerm.ToString(),
                        new Vector2(currentX, currentY),
                        Game1.textColor);

                    currentY += Game1.smallFont.MeasureString(searchTerm.ToString()).ToPoint().Y;
                    break;
            }
        }
    }

    /// <inheritdoc />
    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        if (!this.scrolling) { }
    }

    /// <inheritdoc />
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        this.scrolling = false;
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        base.receiveScrollWheelAction(direction);
        if (direction < 0
            && this.inventory.isWithinBounds(mouseX, mouseY)
            && this.rowOffset < this.totalRows - this.inventory.rows - 1)
        {
            this.rowOffset++;
            this.inventory.actualInventory = this
                .allItems.Skip(this.rowOffset * (this.inventory.capacity / this.inventory.rows))
                .Take(this.inventory.capacity)
                .ToList();

            Game1.playSound("shiny4");
            return;
        }

        if (direction > 0 && this.inventory.isWithinBounds(mouseX, mouseY) && this.rowOffset > 0)
        {
            this.rowOffset--;
            this.inventory.actualInventory = this
                .allItems.Skip(this.rowOffset * (this.inventory.capacity / this.inventory.rows))
                .Take(this.inventory.capacity)
                .ToList();

            Game1.playSound("shiny4");
        }
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

        this.searchBar.Update(x, y);

        if (this.scrollAmount > 0)
        {
            this.arrowUpLeft.tryHover(x, y);
        }

        if (this.scrollAmount < this.scrollArea - 1)
        {
            this.arrowDownLeft.tryHover(x, y);
        }

        if (this.rowOffset > 0)
        {
            this.arrowUpRight.tryHover(x, y);
        }

        if (this.rowOffset < this.totalRows - 1)
        {
            this.arrowDownRight.tryHover(x, y);
        }
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key is Keys.Escape && this.readyToClose())
        {
            this.exitThisMenuNoSound();
        }

        if (key is Keys.Tab && this.searchBar.Selected)
        {
            // Auto-complete on tab
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.searchBar.LeftClick(x, y);
        if (this.searchBar.Selected)
        {
            return;
        }

        if (this.scrollAmount > 0 && this.arrowUpLeft.containsPoint(x, y))
        {
            Game1.playSound("shwip");
            this.scrollAmount--;
            return;
        }

        if (this.rowOffset > 0 && this.arrowUpRight.containsPoint(x, y))
        {
            Game1.playSound("shwip");
            this.rowOffset--;
            this.inventory.actualInventory = this.allItems.Skip(this.rowOffset).Take(this.inventory.capacity).ToList();

            return;
        }

        if (this.scrollAmount < this.scrollArea - 1 && this.arrowDownLeft.containsPoint(x, y))
        {
            Game1.playSound("shwip");
            this.scrollAmount++;
            return;
        }

        if (this.rowOffset < this.totalRows - this.inventory.rows - 1 && this.arrowDownRight.containsPoint(x, y))
        {
            Game1.playSound("shwip");
            this.rowOffset++;
            this.inventory.actualInventory = this
                .allItems.Skip(this.rowOffset * (this.inventory.capacity / this.inventory.rows))
                .Take(this.inventory.capacity)
                .ToList();

            return;
        }

        if (this.scrollBar.containsPoint(x, y)) { }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        this.searchBar.RightClick(x, y);
        if (this.searchBar.Selected)
        {
            return;
        }

        this.searchBar.Selected = false;
    }

    private bool HighlightMethod(Item item) => true;
}