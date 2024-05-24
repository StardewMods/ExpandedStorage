namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>A component for configuring a tab.</summary>
internal sealed class TabEditor : BaseComponent
{
    private readonly TabData data;
    private readonly ClickableTextureComponent downArrow;
    private readonly ClickableTextureComponent icon;
    private readonly ClickableTextureComponent upArrow;

    private EventHandler<TabClickedEventArgs>? clicked;
    private string? hoverText;
    private EventHandler<TabClickedEventArgs>? moveDown;
    private EventHandler<TabClickedEventArgs>? moveUp;

    /// <summary>Initializes a new instance of the <see cref="TabEditor" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="width">The width of the tab component.</param>
    /// <param name="icon">The tab icon.</param>
    /// <param name="tabData">The inventory tab data.</param>
    public TabEditor(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        int x,
        int y,
        int width,
        IIcon icon,
        TabData tabData)
        : base(inputHelper, x, y, width, Game1.tileSize, tabData.Label)
    {
        this.data = tabData;
        this.icon = icon.GetComponent(IconStyle.Transparent, x, y);
        this.icon.bounds = this.bounds with
        {
            X = this.bounds.X + Game1.tileSize,
            Width = this.bounds.Width - (Game1.tileSize * 2),
        };

        this.upArrow = iconRegistry.RequireIcon(VanillaIcon.ArrowUp).GetComponent(IconStyle.Transparent, x + 8, y + 8);

        this.downArrow = iconRegistry
            .RequireIcon(VanillaIcon.ArrowDown)
            .GetComponent(IconStyle.Transparent, x + width - Game1.tileSize + 8, y + 8);

        this.downArrow.hoverText = I18n.Ui_MoveDown_Tooltip();
        this.upArrow.hoverText = I18n.Ui_MoveUp_Tooltip();
    }

    /// <summary>Event triggered when the tab is clicked.</summary>
    public event EventHandler<TabClickedEventArgs> Clicked
    {
        add => this.clicked += value;
        remove => this.clicked -= value;
    }

    /// <summary>Event triggered when the move up button is clicked.</summary>
    public event EventHandler<TabClickedEventArgs> MoveDown
    {
        add => this.moveDown += value;
        remove => this.moveDown -= value;
    }

    /// <summary>Event triggered when the move down button is clicked.</summary>
    public event EventHandler<TabClickedEventArgs> MoveUp
    {
        add => this.moveUp += value;
        remove => this.moveUp -= value;
    }

    /// <inheritdoc />
    public override string? HoverText => this.hoverText;

    /// <summary>Gets or sets a value indicating whether the tab is currently active.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Gets or sets the index.</summary>
    public int Index { get; set; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        cursor -= offset;
        var hover = this.bounds.Contains(cursor);
        var color = this.Active
            ? Color.White
            : hover
                ? Color.LightGray
                : Color.Gray;

        // Top-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.icon.bounds.X + offset.X + 20,
                this.icon.bounds.Y + offset.Y,
                this.icon.bounds.Width - 40,
                this.icon.bounds.Height),
            new Rectangle(21, 368, 6, 16),
            color,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0.5f);

        // Bottom-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.icon.bounds.X + offset.X + 20,
                this.icon.bounds.Y + this.icon.bounds.Height + offset.Y - 20,
                this.icon.bounds.Width - 40,
                20),
            new Rectangle(21, 368, 6, 5),
            color,
            0,
            Vector2.Zero,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.icon.bounds.X + offset.X, this.icon.bounds.Y + offset.Y),
            new Rectangle(16, 368, 5, 15),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            0.5f);

        // Bottom-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.icon.bounds.X + offset.X, this.icon.bounds.Y + this.icon.bounds.Height + offset.Y - 20),
            new Rectangle(16, 368, 5, 5),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Right
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.icon.bounds.Right + offset.X - 20, this.icon.bounds.Y + offset.Y),
            new Rectangle(16, 368, 5, 15),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally,
            0.5f);

        // Bottom-Right
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(
                this.icon.bounds.Right + offset.X - 20,
                this.icon.bounds.Y + this.icon.bounds.Height + offset.Y - 20),
            new Rectangle(16, 368, 5, 5),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
            0.5f);

        if (this.Active && hover)
        {
            this.icon.tryHover(cursor.X, cursor.Y);
        }

        this.icon.draw(spriteBatch, color, 1f, 0, offset.X, offset.Y);

        spriteBatch.DrawString(
            Game1.smallFont,
            this.name,
            new Vector2(
                this.icon.bounds.X + Game1.tileSize + offset.X,
                this.icon.bounds.Y + (IClickableMenu.borderWidth / 2f) + offset.Y),
            this.Active ? Game1.textColor : Game1.unselectedOptionColor);

        if (!this.Active)
        {
            return;
        }

        this.upArrow.tryHover(cursor.X, cursor.Y);
        this.downArrow.tryHover(cursor.X, cursor.Y);

        this.upArrow.draw(spriteBatch, color, 1f);
        this.downArrow.draw(spriteBatch, color, 1f);

        if (this.upArrow.bounds.Contains(cursor))
        {
            this.hoverText = this.upArrow.hoverText;
            return;
        }

        if (this.downArrow.bounds.Contains(cursor))
        {
            this.hoverText = this.downArrow.hoverText;
            return;
        }

        this.hoverText = null;
    }

    /// <inheritdoc />
    public override void MoveTo(Point cursor)
    {
        base.MoveTo(cursor);
        this.icon.bounds.Y = cursor.Y;
        this.upArrow.bounds.Y = cursor.Y + 8;
        this.downArrow.bounds.Y = cursor.Y + 8;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.Active && this.downArrow.bounds.Contains(cursor))
        {
            this.moveDown.InvokeAll(this, new TabClickedEventArgs(SButton.MouseLeft, this.data));
            return true;
        }

        if (this.Active && this.upArrow.bounds.Contains(cursor))
        {
            this.moveUp.InvokeAll(this, new TabClickedEventArgs(SButton.MouseLeft, this.data));
            return true;
        }

        this.clicked.InvokeAll(this, new TabClickedEventArgs(SButton.MouseLeft, this.data));
        return true;
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        if (this.Active && this.downArrow.bounds.Contains(cursor))
        {
            this.moveDown.InvokeAll(this, new TabClickedEventArgs(SButton.MouseRight, this.data));
            return true;
        }

        if (this.Active && this.upArrow.bounds.Contains(cursor))
        {
            this.moveUp.InvokeAll(this, new TabClickedEventArgs(SButton.MouseRight, this.data));
            return true;
        }

        this.clicked.InvokeAll(this, new TabClickedEventArgs(SButton.MouseRight, this.data));
        return true;
    }
}