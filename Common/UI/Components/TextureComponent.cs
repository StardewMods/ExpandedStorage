#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Base custom texture component.</summary>
internal sealed class TextureComponent : ClickableTextureComponent, ICustomComponent
{
    private EventHandler<IClicked>? clicked;

    /// <summary>Initializes a new instance of the <see cref="TextureComponent" /> class.</summary>
    /// <param name="parent">The parent menu.</param>
    /// <param name="name">The component name.</param>
    /// <param name="bounds">The component bounding box.</param>
    /// <param name="texture">The component texture.</param>
    /// <param name="sourceRect">The source rectangle..</param>
    /// <param name="scale">The texture scale.</param>
    public TextureComponent(
        ICustomMenu? parent,
        string name,
        Rectangle bounds,
        Texture2D texture,
        Rectangle sourceRect,
        float scale)
        : base(name, bounds, null, null, texture, sourceRect, scale)
    {
        this.Parent = parent;
        this.Color = Color.White;
    }

    /// <inheritdoc />
    public event EventHandler<IClicked>? Clicked
    {
        add => this.clicked += value;
        remove => this.clicked -= value;
    }

    /// <inheritdoc />
    public Rectangle Bounds => this.bounds;

    /// <inheritdoc />
    public string? HoverText => this.hoverText;

    /// <inheritdoc />
    public Point Location => this.bounds.Location;

    /// <inheritdoc />
    public ICustomMenu? Parent { get; }

    /// <inheritdoc />
    public Point Size => this.bounds.Size;

    /// <inheritdoc />
    public Color Color { get; private set; }

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch, Point cursor, Point offset) =>
        this.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);

    /// <inheritdoc />
    public ICustomComponent MoveTo(Point location)
    {
        this.bounds.Location = location;
        return this;
    }

    /// <inheritdoc />
    public ICustomComponent ResizeTo(Point size)
    {
        this.bounds.Size = size;
        return this;
    }

    /// <inheritdoc />
    public ICustomComponent SetColor(Color value)
    {
        this.Color = value;
        return this;
    }

    /// <inheritdoc />
    public ICustomComponent SetHoverText(string? value)
    {
        this.hoverText = value;
        return this;
    }

    /// <inheritdoc />
    public bool TryLeftClick(Point cursor)
    {
        if (!this.bounds.Contains(cursor))
        {
            return false;
        }

        this.clicked.InvokeAll(this, new ClickedEventArgs(SButton.MouseLeft, cursor));
        return true;
    }

    /// <inheritdoc />
    public bool TryRightClick(Point cursor)
    {
        if (!this.bounds.Contains(cursor))
        {
            return false;
        }

        this.clicked.InvokeAll(this, new ClickedEventArgs(SButton.Right, cursor));
        return true;
    }

    /// <inheritdoc />
    public bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public void Update(Point cursor) => this.tryHover(cursor.X, cursor.Y);
}