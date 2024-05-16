namespace StardewMods.Common.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>A dropdown for selecting a string from a list of values.</summary>
internal abstract class BaseDropdown : BaseMenu
{
    private readonly Rectangle bounds;
    private readonly List<string> items;

    /// <summary>Initializes a new instance of the <see cref="BaseDropdown" /> class.</summary>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="items">The dropdown list items.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    protected BaseDropdown(ClickableComponent anchor, IReadOnlyCollection<string> items, int maxItems = int.MaxValue)
        : base(0, 0, 0, 0)
    {
        this.items = items.Where(item => item.Trim().Length >= 3).ToList();
        var textBounds = items.Select(item => Game1.smallFont.MeasureString(item).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);

        this.width = textBounds.Max(textBound => textBound.X) + 16;
        this.height = textBounds.Take(maxItems).Sum(textBound => textBound.Y) + 16;

        this.xPositionOnScreen = anchor.bounds.Left;
        this.yPositionOnScreen = anchor.bounds.Bottom;
        if (this.xPositionOnScreen + this.width > Game1.uiViewport.Width)
        {
            this.xPositionOnScreen = anchor.bounds.Right - this.width;
        }

        if (this.yPositionOnScreen + this.height > Game1.uiViewport.Height)
        {
            this.yPositionOnScreen = anchor.bounds.Top - this.height;
        }

        this.bounds = new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        this.Components = items
            .Take(maxItems)
            .Select(
                (_, index) => new ClickableComponent(
                    new Rectangle(
                        this.bounds.X + 8,
                        this.bounds.Y + 8 + (textHeight * index),
                        this.bounds.Width,
                        textHeight),
                    index.ToString(CultureInfo.InvariantCulture)))
            .ToList();
    }

    /// <summary>Gets the components.</summary>
    protected List<ClickableComponent> Components { get; }

    /// <summary>Gets the offset.</summary>
    protected int Offset { get; private set; }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        // Scroll down
        if (direction < 0)
        {
            this.Offset--;
            Game1.playSound("shiny4");
        }

        // Scroll up
        if (direction > 0)
        {
            this.Offset++;
            Game1.playSound("shiny4");
        }

        this.Offset = Math.Min(Math.Max(this.Offset, 0), Math.Max(this.items.Count - this.Components.Count, 0));
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch b)
    {
        // Draw items
        var (x, y) = Game1.getMousePosition(true);
        foreach (var component in this.Components)
        {
            var index = this.Offset + int.Parse(component.name, CultureInfo.InvariantCulture);
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

            b.DrawString(Game1.smallFont, item, new Vector2(component.bounds.X, component.bounds.Y), Game1.textColor);
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch b) =>
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

    /// <summary>Performs a left click at the specified location and returns the result.</summary>
    /// <param name="x">The x-coordinate of the location to click.</param>
    /// <param name="y">The y-coordinate of the location to click.</param>
    /// <returns>Returns the index of the selected item, or -1 if an item was not clicked.</returns>
    protected int LeftClick(int x, int y)
    {
        var component = this.Components.FirstOrDefault(i => i.bounds.Contains(x, y));
        if (component is null)
        {
            return -1;
        }

        return this.Offset + int.Parse(component.name, CultureInfo.InvariantCulture);
    }
}