namespace StardewMods.Common.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

/// <summary>A popup menu for selecting from a list of values.</summary>
internal class BasePopupList : BaseMenu
{
    private readonly Rectangle bounds;
    private readonly List<ClickableComponent> components;
    private readonly List<string> items;
    private readonly TextField textField;

    private string currentText;
    private int offset;

    /// <summary>Initializes a new instance of the <see cref="BasePopupList" /> class.</summary>
    /// <param name="initialText">The initial text.</param>
    /// <param name="items">The popup list items.</param>
    public BasePopupList(string initialText, List<string> items)
        : base(
            (Game1.uiViewport.Width / 2) - ((400 + (IClickableMenu.borderWidth * 2)) / 2),
            (Game1.uiViewport.Height / 2) - ((480 + (IClickableMenu.borderWidth * 2)) / 2),
            400,
            480)
    {
        this.items = items;
        this.currentText = initialText;
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

        var textBounds = items.Select(item => Game1.smallFont.MeasureString(item).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);
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

        // ok button
    }

    /// <summary>Gets or sets the current text.</summary>
    public string CurrentText
    {
        get => this.currentText;
        set
        {
            this.currentText = value;
            var newItems = new List<string>(
                this
                    .items.OrderByDescending(i => i.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(i => i));

            this.items.Clear();
            this.items.AddRange(newItems);
        }
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        base.draw(b);

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
        var (x, y) = Game1.getMousePosition(true);
        foreach (var component in this.components)
        {
            var index = this.offset + int.Parse(component.name, CultureInfo.InvariantCulture);
            var item = this.items.ElementAtOrDefault(index);
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            if (component.bounds.Contains(x, y))
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

        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
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

        this.textField.LeftClick(x, y);
        if (this.textField.Selected)
        {
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
        this.textField.RightClick(x, y);
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
            this.offset--;
            Game1.playSound("shiny4");
        }

        // Scroll up
        if (direction > 0)
        {
            this.offset++;
            Game1.playSound("shiny4");
        }

        this.offset = Math.Max(0, Math.Min(this.items.Count - this.components.Count, 0));
    }

    /// <summary>Performs a left click at the specified location and returns the result.</summary>
    /// <param name="x">The x-coordinate of the location to click.</param>
    /// <param name="y">The y-coordinate of the location to click.</param>
    /// <returns>Returns the index of the selected item, or -1 if an item was not clicked.</returns>
    protected int LeftClick(int x, int y)
    {
        var component = this.components.FirstOrDefault(i => i.bounds.Contains(x, y));
        if (component is null)
        {
            return -1;
        }

        return this.offset + int.Parse(component.name, CultureInfo.InvariantCulture);
    }
}