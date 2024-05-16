namespace StardewMods.Common.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

/// <summary>A popup menu for selecting from a list of values.</summary>
internal class PopupList : BaseMenu
{
    private readonly Rectangle bounds;
    private readonly ClickableTextureComponent buttonCancel;
    private readonly ClickableTextureComponent buttonOk;
    private readonly Action<string> callback;
    private readonly List<ClickableComponent> components;
    private readonly List<string> items;
    private readonly TextField textField;

    private string currentText;
    private int offset;

    /// <summary>Initializes a new instance of the <see cref="PopupList" /> class.</summary>
    /// <param name="initialText">The initial text.</param>
    /// <param name="items">The popup list items.</param>
    /// <param name="callback">The action to call when a value is selected.</param>
    public PopupList(string initialText, IReadOnlyCollection<string> items, Action<string> callback)
        : base(0, 0, 400, 480)
    {
        this.items = items.Where(item => item.Trim().Length >= 3).ToList();
        var textBounds = items.Select(item => Game1.smallFont.MeasureString(item).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);

        this.width = Math.Max(400, textBounds.Max(textBound => textBound.X) + 16);
        this.xPositionOnScreen = (Game1.uiViewport.Width / 2) - ((this.width + (IClickableMenu.borderWidth * 2)) / 2);
        this.yPositionOnScreen = (Game1.uiViewport.Height / 2) - ((480 + (IClickableMenu.borderWidth * 2)) / 2);

        this.currentText = initialText;
        this.callback = callback;
        this.textField = new TextField(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + Game1.tileSize,
            this.width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            () => this.CurrentText,
            value => this.CurrentText = value)
        {
            Selected = true,
        };

        this.bounds = new Rectangle(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            this.yPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 112,
            this.width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            this.height - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth - 112);

        var maxItems = this.bounds.Height / textHeight;
        this.bounds.Height = (maxItems * textHeight) + 16;
        this.components = items
            .Take(maxItems)
            .Select(
                (item, index) => new ClickableComponent(
                    new Rectangle(
                        this.bounds.X + 8,
                        this.bounds.Y + 8 + (textHeight * index),
                        this.bounds.Width,
                        textHeight),
                    index.ToString(CultureInfo.InvariantCulture),
                    item))
            .ToList();

        this.RefreshItems();

        this.buttonOk = new ClickableTextureComponent(
            "Ok",
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + this.height - (IClickableMenu.borderWidth / 2) - Game1.tileSize,
                Game1.tileSize,
                Game1.tileSize),
            null,
            null,
            Game1.mouseCursors,
            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46),
            1f);

        this.buttonCancel = new ClickableTextureComponent(
            "Cancel",
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + this.height - (IClickableMenu.borderWidth / 2) - (Game1.tileSize * 2) - 16,
                Game1.tileSize,
                Game1.tileSize),
            null,
            null,
            Game1.mouseCursors,
            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47),
            1f);

        this.allClickableComponents.Add(this.buttonOk);
        this.allClickableComponents.Add(this.buttonCancel);
    }

    /// <summary>Gets or sets the current text.</summary>
    public string CurrentText
    {
        get => this.currentText;
        set
        {
            this.currentText = value;
            this.RefreshItems();
        }
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.textField.Update(x, y);
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key is Keys.Escape && this.readyToClose())
        {
            this.exitThisMenuNoSound();
            return;
        }

        if (key is Keys.Enter && this.readyToClose())
        {
            this.exitThisMenuNoSound();
            return;
        }

        if (key is Keys.Tab && this.textField.Selected)
        {
            // Auto-complete on tab?
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        this.textField.TryLeftClick(x, y);
        if (this.textField.Selected)
        {
            return;
        }

        if (this.buttonOk.containsPoint(x, y) && this.readyToClose())
        {
            this.callback(this.CurrentText);
            this.exitThisMenuNoSound();
            return;
        }

        if (this.buttonCancel.containsPoint(x, y) && this.readyToClose())
        {
            this.exitThisMenuNoSound();
            return;
        }

        var component = this.components.FirstOrDefault(i => i.bounds.Contains(x, y));
        if (component is null)
        {
            return;
        }

        var selectedIndex = this.offset + int.Parse(component.name, CultureInfo.InvariantCulture);
        var selectedItem = this.items.ElementAtOrDefault(selectedIndex);
        if (string.IsNullOrWhiteSpace(selectedItem))
        {
            return;
        }

        this.CurrentText = selectedItem;
        this.textField.Reset();
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        this.textField.TryRightClick(x, y);
        if (this.textField.Selected)
        {
            return;
        }

        this.textField.Selected = false;
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.bounds.Contains(mouseX, mouseY))
        {
            return;
        }

        // Scroll down
        if (direction < 0)
        {
            this.offset++;
            Game1.playSound("shiny4");
        }

        // Scroll up
        if (direction > 0)
        {
            this.offset--;
            Game1.playSound("shiny4");
        }

        this.offset = Math.Max(0, Math.Min(this.items.Count - this.components.Count, this.offset));
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch b)
    {
        // Draw background
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            this.bounds.X,
            this.bounds.Y,
            this.bounds.Width,
            this.bounds.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            0.97f);

        // Draw text field
        this.textField.Draw(b);

        // Draw items
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        foreach (var component in this.components)
        {
            var index = this.offset + int.Parse(component.name, CultureInfo.InvariantCulture);
            var item = this.items.ElementAtOrDefault(index);
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            if (component.bounds.Contains(mouseX, mouseY))
            {
                b.Draw(
                    Game1.staminaRect,
                    component.bounds with { Width = component.bounds.Width - 16 },
                    new Rectangle(0, 0, 1, 1),
                    Color.Wheat,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.975f);
            }

            b.DrawString(
                Game1.smallFont,
                item,
                new Vector2(component.bounds.X, component.bounds.Y),
                item.Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase)
                    ? Game1.textColor
                    : Game1.unselectedOptionColor);
        }
    }

    private void RefreshItems()
    {
        var newItems = new List<string>(
            this
                .items.OrderByDescending(i => i.Contains(this.currentText, StringComparison.OrdinalIgnoreCase))
                .ThenBy(i => i));

        this.items.Clear();
        this.items.AddRange(newItems);
    }
}