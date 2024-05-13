namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewValley.Menus;

/// <summary>A menu for editing search.</summary>
internal sealed class SearchMenu : BaseMenu
{
    private readonly ExpressionEditor expressionEditor;
    private readonly IExpressionHandler expressionHandler;
    private readonly InventoryMenu inventory;
    private readonly VerticalScrollBar scrollExpressions;
    private readonly VerticalScrollBar scrollInventory;
    private readonly TextField textField;
    private readonly UiManager uiManager;

    private List<Item> allItems = [];
    private int rowOffset;
    private IExpression? searchExpression;
    private string searchText;
    private int totalRows;

    /// <summary>Initializes a new instance of the <see cref="SearchMenu" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="searchText">The initial search text.</param>
    /// <param name="uiManager">Dependency used for managing ui.</param>
    public SearchMenu(
        AssetHandler assetHandler,
        IExpressionHandler expressionHandler,
        string searchText,
        UiManager uiManager)
    {
        this.expressionHandler = expressionHandler;
        this.uiManager = uiManager;
        this.expressionEditor = new ExpressionEditor(
            this,
            this.expressionHandler,
            this.xPositionOnScreen + IClickableMenu.borderWidth,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            340,
            448);

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

        this.inventory.populateClickableComponentList();

        this.searchText = searchText;
        this.ParseSearch();
        this.RefreshItems();

        this.textField = new TextField(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + Game1.tileSize,
            this.width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            () => this.searchText,
            value =>
            {
                this.searchText = value;
                this.ParseSearch();
                this.RefreshItems();
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

        this.populateClickableComponentList();
    }

    /// <summary>Gets or sets the dropdown.</summary>
    public BaseDropdown? DropDown { get; set; }

    /// <summary>Gets or sets the hover text.</summary>
    public string? HoverText { get; set; }

    /// <inheritdoc />
    public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        base.draw(b, -1);
        this.HoverText = null;

        this.drawHorizontalPartition(
            b,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.drawVerticalIntersectingPartition(
            b,
            this.xPositionOnScreen + (IClickableMenu.borderWidth / 2) + 400,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.uiManager.DrawInFrame(
            b,
            SpriteSortMode.Deferred,
            new Rectangle(
                this.expressionEditor.xPositionOnScreen - 4,
                this.expressionEditor.yPositionOnScreen - 8,
                this.expressionEditor.width + 8,
                this.expressionEditor.height + 16),
            () =>
            {
                this.expressionEditor.draw(b);
            });

        this.textField.Draw(b);
        this.inventory.draw(b);
        this.scrollExpressions.Draw(b);
        this.scrollInventory.Draw(b);

        if (this.GetChildMenu() is not null)
        {
            return;
        }

        if (this.DropDown is not null)
        {
            this.DropDown.draw(b);
        }
        else if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(b, this.HoverText, string.Empty, null);
        }
        else
        {
            var (mouseX, mouseY) = Game1.getMousePosition(true);
            var item = this.inventory.hover(mouseX, mouseY, null);
            if (item is not null)
            {
                IClickableMenu.drawToolTip(b, this.inventory.descriptionText, this.inventory.descriptionTitle, item);
            }
        }

        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
    }

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

    /// <summary>Updates the search text without parsing.</summary>
    /// <param name="value">The new search text value.</param>
    public void OverrideSearchText(string value)
    {
        this.searchText = value;
        this.textField.Reset();
        this.RefreshItems();
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.textField.Update(x, y);
        this.scrollExpressions.Update(x, y);
        this.scrollInventory.Update(x, y);
        this.expressionEditor.performHoverAction(x, y);
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
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (this.DropDown is not null)
        {
            this.DropDown.receiveLeftClick(x, y, playSound);
            return;
        }

        this.textField.LeftClick(x, y);
        if (this.textField.Selected)
        {
            return;
        }

        if (this.scrollExpressions.Click(x, y))
        {
            return;
        }

        if (this.scrollInventory.Click(x, y))
        {
            return;
        }

        this.expressionEditor.receiveLeftClick(x, y);
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);

        if (this.DropDown is not null)
        {
            this.DropDown.receiveRightClick(x, y, playSound);
            return;
        }

        this.textField.RightClick(x, y);
        if (this.textField.Selected)
        {
            return;
        }

        this.textField.Selected = false;
        this.expressionEditor.receiveRightClick(x, y);
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.DropDown is not null)
        {
            this.DropDown.receiveScrollWheelAction(direction);
            return;
        }

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

    private bool HighlightMethod(Item item) => true;

    private void ParseSearch()
    {
        this.searchExpression = this.expressionHandler.TryParseExpression(this.searchText, out var expression)
            ? expression
            : null;

        this.expressionEditor.ReInitializeComponents(this.searchExpression);
    }

    private void RefreshItems()
    {
        if (this.searchExpression is null)
        {
            this.allItems = [];
            this.inventory.actualInventory.Clear();
            this.totalRows = 0;
            return;
        }

        this.allItems = ItemRepository.GetItems(this.searchExpression.Equals).ToList();
        this.inventory.actualInventory = this.allItems.Take(this.inventory.capacity).ToList();
        this.totalRows = (int)Math.Ceiling(
            (float)this.allItems.Count / (this.inventory.capacity / this.inventory.rows));
    }
}