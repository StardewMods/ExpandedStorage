namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>Represents a component with an icon that expands into a label when hovered.</summary>
internal sealed class TabIcon : BaseComponent
{
    private readonly TabData data;
    private readonly Vector2 origin;
    private readonly int textWidth;

    private EventHandler<TabData>? clicked;

    /// <summary>Initializes a new instance of the <see cref="TabIcon" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="icon">The tab icon.</param>
    /// <param name="tabData">The inventory tab data.</param>
    public TabIcon(IInputHelper inputHelper, int x, int y, IIcon icon, TabData tabData)
        : base(inputHelper, x, y, Game1.tileSize, Game1.tileSize, tabData.Label)
    {
        var textBounds = Game1.smallFont.MeasureString(tabData.Label).ToPoint();
        this.data = tabData;
        this.origin = new Vector2(x, y);
        this.Component = icon.GetComponent(IconStyle.Transparent);
        this.Component.bounds = this.bounds;
        this.Component.name = this.name;
        this.textWidth = textBounds.X;
    }

    /// <summary>Event triggered when the tab is clicked.</summary>
    public event EventHandler<TabData> Clicked
    {
        add => this.clicked += value;
        remove => this.clicked -= value;
    }

    /// <inheritdoc />
    public override ClickableComponent Component { get; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        // Top-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.Component.bounds.X + 20,
                this.Component.bounds.Y,
                this.Component.bounds.Width - 40,
                this.Component.bounds.Height),
            new Rectangle(21, 368, 6, 16),
            Color.White,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0.5f);

        // Bottom-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.Component.bounds.X + 20,
                this.Component.bounds.Y + this.Component.bounds.Height - 20,
                this.Component.bounds.Width - 40,
                20),
            new Rectangle(21, 368, 6, 5),
            Color.White,
            0,
            Vector2.Zero,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Component.bounds.X, this.Component.bounds.Y),
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
            new Vector2(this.Component.bounds.X, this.Component.bounds.Y + this.Component.bounds.Height - 20),
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
            new Vector2(this.Component.bounds.Right - 20, this.Component.bounds.Y),
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
            new Vector2(this.Component.bounds.Right - 20, this.Component.bounds.Y + this.Component.bounds.Height - 20),
            new Rectangle(16, 368, 5, 5),
            Color.White,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
            0.5f);

        ((ClickableTextureComponent)this.Component).draw(spriteBatch);

        if (this.Component.bounds.Width == this.textWidth + Game1.tileSize + IClickableMenu.borderWidth)
        {
            spriteBatch.DrawString(
                Game1.smallFont,
                this.Component.name,
                new Vector2(
                    this.Component.bounds.X + Game1.tileSize,
                    this.Component.bounds.Y + (IClickableMenu.borderWidth / 2f)),
                Color.Black);
        }
    }

    /// <inheritdoc />
    public override bool TryLeftClick(int x, int y)
    {
        this.clicked.InvokeAll(this, this.data);
        return true;
    }

    /// <inheritdoc />
    public override bool TryRightClick(int x, int y)
    {
        this.clicked.InvokeAll(this, this.data);
        return true;
    }

    /// <inheritdoc />
    public override void Update(int mouseX, int mouseY)
    {
        this.Component.bounds.Width = this.Component.bounds.Contains(mouseX, mouseY)
            ? Math.Min(this.Component.bounds.Width + 16, this.textWidth + Game1.tileSize + IClickableMenu.borderWidth)
            : Math.Max(this.Component.bounds.Width - 16, Game1.tileSize);

        this.Component.bounds.X = (int)this.origin.X - this.Component.bounds.Width;
    }
}