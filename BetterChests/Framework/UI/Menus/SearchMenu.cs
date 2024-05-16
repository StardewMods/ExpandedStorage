namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewValley.Menus;

/// <summary>A menu for editing search.</summary>
internal class SearchMenu : BaseMenu
{
    private readonly ExpressionEditor expressionEditor;
    private readonly IExpressionHandler expressionHandler;
    private readonly InventoryMenu inventory;
    private readonly VerticalScrollBar scrollExpressions;
    private readonly VerticalScrollBar scrollInventory;
    private readonly TextField textField;

    private List<Item> allItems = [];
    private int rowOffset;
    private IExpression? searchExpression;
    private int totalRows;

    /// <summary>Initializes a new instance of the <see cref="SearchMenu" /> class.</summary>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="searchText">The initial search text.</param>
    /// <param name="uiManager">Dependency used for managing ui.</param>
    public SearchMenu(IExpressionHandler expressionHandler, string searchText, UiManager uiManager)
    {
        this.expressionHandler = expressionHandler;
        this.expressionEditor = new ExpressionEditor(
            this.expressionHandler,
            uiManager,
            () => this.SearchText!,
            value => this.SetSearchText(value),
            this.xPositionOnScreen + IClickableMenu.borderWidth,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            340,
            448);

        this.AddSubMenu(this.expressionEditor);

        this.inventory = new InventoryMenu(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 428,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            false,
            new List<Item>(),
            this.HighlightMethod,
            35,
            7);

        this.AddSubMenu(this.inventory);
        this.SetSearchText(searchText, true);

        this.textField = new TextField(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + Game1.tileSize,
            this.width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            () => this.SearchText,
            value =>
            {
                this.SetSearchText(value, true);
            });

        this.scrollExpressions = new VerticalScrollBar(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 348,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 16,
            448,
            () => this.expressionEditor.OffsetY,
            value =>
            {
                this.expressionEditor.OffsetY = value;
            },
            () => 0,
            () => this.expressionEditor.MaxOffset,
            40);

        this.scrollInventory = new VerticalScrollBar(
            this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - Game1.tileSize - 12,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 16,
            448,
            () => this.rowOffset,
            value =>
            {
                this.rowOffset = value;
                this.inventory.actualInventory = this
                    .allItems.Skip(this.rowOffset * (this.inventory.capacity / this.inventory.rows))
                    .Take(this.inventory.capacity)
                    .ToList();
            },
            () => 0,
            () => this.totalRows - this.inventory.rows - 1);

        this.allClickableComponents.Add(this.textField);
        this.allClickableComponents.Add(this.scrollExpressions);
        this.allClickableComponents.Add(this.scrollInventory);
    }

    /// <summary>Gets the current search text.</summary>
    public string SearchText { get; private set; }

    /// <inheritdoc />
    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        if (this.scrollExpressions.IsActive)
        {
            this.scrollExpressions.Update(x, y);
            return;
        }

        if (this.scrollInventory.IsActive)
        {
            this.scrollInventory.Update(x, y);
        }
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key is Keys.Escape && this.readyToClose())
        {
            this.exitThisMenuNoSound();
        }

        if (key is Keys.Tab && this.textField.Selected)
        {
            // Auto-complete on tab
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.expressionEditor.isWithinBounds(mouseX, mouseY))
        {
            this.scrollExpressions.Scroll(direction);
            return;
        }

        if (this.inventory.isWithinBounds(mouseX, mouseY))
        {
            this.scrollInventory.Scroll(direction);
        }
    }

    /// <inheritdoc />
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        this.scrollExpressions.UnClick(x, y);
        this.scrollInventory.UnClick(x, y);
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch spriteBatch)
    {
        this.drawHorizontalPartition(
            spriteBatch,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.drawVerticalIntersectingPartition(
            spriteBatch,
            this.xPositionOnScreen + (IClickableMenu.borderWidth / 2) + 400,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);
    }

    /// <inheritdoc />
    protected override void DrawOver(SpriteBatch spriteBatch)
    {
        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        var item = this.inventory.hover(mouseX, mouseY, null);
        if (item is not null)
        {
            IClickableMenu.drawToolTip(
                spriteBatch,
                this.inventory.descriptionText,
                this.inventory.descriptionTitle,
                item);
        }
    }

    /// <summary>Get the items that should be displayed in the menu.</summary>
    /// <returns>The items to display.</returns>
    protected virtual List<Item> GetItems() =>
        this.searchExpression is null
            ? Array.Empty<Item>().ToList()
            : ItemRepository.GetItems(this.searchExpression.Equals).ToList();

    /// <summary>Highlight the item.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>A value indicating whether the item should be highlighted.</returns>
    protected virtual bool HighlightMethod(Item item) => InventoryMenu.highlightAllItems(item);

    /// <summary>Updates the search text without parsing.</summary>
    /// <param name="value">The new search text value.</param>
    /// <param name="parse">Indicates whether to parse the text.</param>
    [MemberNotNull(nameof(SearchMenu.SearchText))]
    protected void SetSearchText(string value, bool parse = false)
    {
        this.SearchText = value;
        if (parse)
        {
            this.searchExpression = this.expressionHandler.TryParseExpression(this.SearchText, out var expression)
                ? expression
                : null;

            this.expressionEditor.ReInitializeComponents(this.searchExpression);
        }

        this.textField?.Reset();
        this.allItems = this.GetItems();
        if (!this.allItems.Any())
        {
            this.inventory.actualInventory.Clear();
            this.totalRows = 0;
            return;
        }

        this.inventory.actualInventory = this.allItems.Take(this.inventory.capacity).ToList();
        this.totalRows = (int)Math.Ceiling(
            (float)this.allItems.Count / (this.inventory.capacity / this.inventory.rows));
    }
}