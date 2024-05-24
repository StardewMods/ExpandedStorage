namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>A menu for editing search.</summary>
internal class SearchMenu : BaseMenu
{
    private readonly ExpressionEditorNew expressionEditor;
    private readonly IExpressionHandler expressionHandler;
    private readonly InventoryMenu inventory;
    private readonly VerticalScrollBar scrollInventory;
    private readonly TextField textField;

    private List<Item> allItems = [];
    private int rowOffset;
    private int totalRows;

    /// <summary>Initializes a new instance of the <see cref="SearchMenu" /> class.</summary>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="searchText">The initial search text.</param>
    public SearchMenu(
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        string searchText)
        : base(inputHelper)
    {
        this.expressionHandler = expressionHandler;

        // this.expressionEditor = new ExpressionEditor(
        //     this.expressionHandler,
        //     iconRegistry,
        //     inputHelper,
        //     reflectionHelper,
        //     () => this.SearchText!,
        //     value => this.SetSearchText(value),
        //     this.xPositionOnScreen + IClickableMenu.borderWidth,
        //     this.yPositionOnScreen
        //     + IClickableMenu.spaceToClearSideBorder
        //     + (IClickableMenu.borderWidth / 2)
        //     + (Game1.tileSize * 2)
        //     + 12,
        //     340,
        //     448);
        this.expressionEditor = new ExpressionEditorNew(
            this.expressionHandler,
            iconRegistry,
            inputHelper,
            reflectionHelper,
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

        this.scrollInventory = new VerticalScrollBar(
            this.Input,
            this.inventory.xPositionOnScreen + this.inventory.width + 4,
            this.inventory.yPositionOnScreen + 4,
            this.inventory.height,
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
            () => Math.Max(0, this.totalRows - this.inventory.rows - 1));

        this.AddSubMenu(this.inventory);
        this.SetSearchText(searchText, true);

        this.textField = new TextField(
            this.Input,
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

        this.allClickableComponents.Add(this.textField);
        this.allClickableComponents.Add(this.scrollInventory);
    }

    /// <summary>Gets the current search text.</summary>
    public string SearchText { get; private set; }

    /// <summary>Gets the current search expression.</summary>
    protected IExpression? Expression { get; private set; }

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
    protected override void Draw(SpriteBatch spriteBatch, Point cursor)
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
    protected override void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        var item = this.inventory.hover(cursor.X, cursor.Y, null);
        if (item is not null)
        {
            IClickableMenu.drawToolTip(
                spriteBatch,
                this.inventory.descriptionText,
                this.inventory.descriptionTitle,
                item);
        }

        base.DrawOver(spriteBatch, cursor);
    }

    /// <summary>Get the items that should be displayed in the menu.</summary>
    /// <returns>The items to display.</returns>
    protected virtual List<Item> GetItems() =>
        this.Expression is null
            ? Array.Empty<Item>().ToList()
            : ItemRepository.GetItems(this.Expression.Equals).ToList();

    /// <summary>Highlight the item.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>A value indicating whether the item should be highlighted.</returns>
    protected virtual bool HighlightMethod(Item item) => InventoryMenu.highlightAllItems(item);

    /// <summary>Refresh the displayed items.</summary>
    protected void RefreshItems()
    {
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

    /// <summary>Updates the search text without parsing.</summary>
    /// <param name="value">The new search text value.</param>
    /// <param name="parse">Indicates whether to parse the text.</param>
    [MemberNotNull(nameof(SearchMenu.SearchText))]
    protected void SetSearchText(string value, bool parse = false)
    {
        this.SearchText = value;
        if (parse)
        {
            this.Expression = this.expressionHandler.TryParseExpression(this.SearchText, out var expression)
                ? expression
                : null;

            this.expressionEditor.ReInitializeComponents(this.Expression);
        }

        this.textField?.Reset();
        this.RefreshItems();
    }

    /// <inheritdoc />
    protected override bool TryScroll(int direction)
    {
        var cursor = this.Input.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        return this.inventory.isWithinBounds(cursor.X, cursor.Y) && this.scrollInventory.TryScroll(direction);
    }
}