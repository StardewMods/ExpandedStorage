namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewValley.Menus;

/// <summary>Represents a component with an icon that expands into a label when hovered.</summary>
internal sealed class TabIcon : ClickableComponent
{
    private readonly ClickableTextureComponent icon;
    private readonly Action onClick;
    private readonly Vector2 origin;
    private readonly int textWidth;

    /// <summary>Initializes a new instance of the <see cref="TabIcon" /> class.</summary>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="icon">The tab icon.</param>
    /// <param name="tabData">The inventory tab data.</param>
    /// <param name="onClick">Action to perform when clicked.</param>
    public TabIcon(int x, int y, Icon icon, TabData tabData, Action onClick)
        : base(
            new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
            ((int)Math.Pow(y, 2) + x).ToString(CultureInfo.InvariantCulture),
            tabData.Label)
    {
        this.onClick = onClick;
        this.myID = (int)(Math.Pow(y, 2) + x);
        this.Data = tabData;
        this.origin = new Vector2(x, y);
        this.icon = new ClickableTextureComponent(
            new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
            Game1.content.Load<Texture2D>(icon.Path),
            icon.Area,
            Game1.pixelZoom);

        var textBounds = Game1.smallFont.MeasureString(tabData.Label).ToPoint();
        this.textWidth = textBounds.X;
    }

    /// <summary>Gets the inventory tab data.</summary>
    public TabData Data { get; }

    /// <summary>Draw the color picker.</summary>
    /// <param name="spriteBatch">The sprite batch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        // Top-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(this.bounds.X + 20, this.bounds.Y, this.bounds.Width - 40, this.bounds.Height),
            new Rectangle(21, 368, 6, 16),
            Color.White,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0.5f);

        // Bottom-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(this.bounds.X + 20, this.bounds.Y + this.bounds.Height - 20, this.bounds.Width - 40, 20),
            new Rectangle(21, 368, 6, 5),
            Color.White,
            0,
            Vector2.Zero,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.bounds.X, this.bounds.Y),
            new Rectangle(16, 368, 5, 15),
            Color.White,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            0.5f);

        // Bottom-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.bounds.X, this.bounds.Y + this.bounds.Height - 20),
            new Rectangle(16, 368, 5, 5),
            Color.White,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Right
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.bounds.Right - 20, this.bounds.Y),
            new Rectangle(16, 368, 5, 15),
            Color.White,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally,
            0.5f);

        // Bottom-Right
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.bounds.Right - 20, this.bounds.Y + this.bounds.Height - 20),
            new Rectangle(16, 368, 5, 5),
            Color.White,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
            0.5f);

        this.icon.draw(spriteBatch);

        if (this.bounds.Width == this.textWidth + Game1.tileSize + IClickableMenu.borderWidth)
        {
            spriteBatch.DrawString(
                Game1.smallFont,
                this.label,
                new Vector2(this.bounds.X + Game1.tileSize, this.bounds.Y + (IClickableMenu.borderWidth / 2)),
                Color.Black);
        }
    }

    /// <summary>Performs a left-click action based on the given mouse coordinates.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse.</param>
    /// <param name="mouseY">The y-coordinate of the mouse.</param>
    /// <returns>true if the left-click action was successfully performed; otherwise, false.</returns>
    public bool LeftClick(int mouseX, int mouseY)
    {
        if (!this.bounds.Contains(mouseX, mouseY))
        {
            return false;
        }

        this.onClick();
        return true;
    }

    /// <summary>Performs a right-click action based on the given mouse coordinates.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse.</param>
    /// <param name="mouseY">The y-coordinate of the mouse.</param>
    /// <returns>true if the right-click action was successfully performed; otherwise, false.</returns>
    public bool RightClick(int mouseX, int mouseY)
    {
        if (!this.bounds.Contains(mouseX, mouseY))
        {
            return false;
        }

        this.onClick();
        return true;
    }

    /// <summary>Updates the tab component based on the mouse position.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse position.</param>
    /// <param name="mouseY">The y-coordinate of the mouse position.</param>
    public void Update(int mouseX, int mouseY)
    {
        this.bounds.Width = this.bounds.Contains(mouseX, mouseY)
            ? Math.Min(this.bounds.Width + 16, this.textWidth + Game1.tileSize + IClickableMenu.borderWidth)
            : Math.Max(this.bounds.Width - 16, Game1.tileSize);

        this.bounds.X = (int)this.origin.X - this.bounds.Width;
        this.icon.bounds.X = this.bounds.X;
    }
}