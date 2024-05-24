#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

internal sealed class TextureComponent : ClickableTextureComponent, ICustomComponent
{
    /// <summary>Represents a component that renders a texture on the screen.</summary>
    /// <param name="name">The component name.</param>
    /// <param name="bounds">The component bounding box.</param>
    /// <param name="texture">The component texture.</param>
    /// <param name="sourceRect">The source rectangle..</param>
    /// <param name="scale">The texture scale.</param>
    public TextureComponent(string name, Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale)
        : base(name, bounds, null, null, texture, sourceRect, scale) { }

    /// <inheritdoc />
    public string? HoverText => this.hoverText;

    /// <inheritdoc />
    public Point Position => this.bounds.Location;

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch, Point cursor, Point offset) =>
        this.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);

    /// <inheritdoc />
    public void DrawInFrame(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        // Do nothing
    }

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
    public ICustomComponent SetHoverText(string? value)
    {
        this.hoverText = value;
        return this;
    }

    /// <inheritdoc />
    public bool TryLeftClick(Point cursor) => this.bounds.Contains(cursor);

    /// <inheritdoc />
    public bool TryRightClick(Point cursor) => this.bounds.Contains(cursor);

    /// <inheritdoc />
    public bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public void Update(Point cursor) => this.tryHover(cursor.X, cursor.Y);
}