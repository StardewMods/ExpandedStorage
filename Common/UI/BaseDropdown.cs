namespace StardewMods.Common.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>A dropdown for selecting a string from a list of values.</summary>
internal abstract class BaseDropdown : IClickableMenu
{
    private readonly Rectangle bounds;
    private readonly List<ClickableComponent> components;
    private readonly List<string> items;

    private int offset;

    /// <summary>Initializes a new instance of the <see cref="BaseDropdown" /> class.</summary>
    /// <param name="items">The dropdown list items.</param>
    /// <param name="x">The x-coordinate of the dropdown.</param>
    /// <param name="y">The y-coordinate of the dropdown.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    protected BaseDropdown(List<string> items, int x, int y, int maxItems = int.MaxValue)
        : base(x, y, 0, 0)
    {
        this.items = items.Where(item => item.Trim().Length >= 3).ToList();
        var textBounds = items.Select(item => Game1.smallFont.MeasureString(item).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);

        this.width = textBounds.Max(textBound => textBound.X) + 16;
        this.height = textBounds.Take(maxItems).Sum(textBound => textBound.Y) + 16;
        this.bounds = new Rectangle(x, y, this.width, this.height);
        this.components = items
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

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
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

            b.DrawString(Game1.smallFont, item, new Vector2(component.bounds.X, component.bounds.Y), Game1.textColor);
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
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

        this.offset = Math.Min(Math.Max(this.offset, 0), Math.Max(this.items.Count - this.components.Count, 0));
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