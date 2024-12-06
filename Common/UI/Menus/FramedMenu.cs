#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else

namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

#endif

/// <summary>Framed menu with vertical scrolling.</summary>
internal abstract class FramedMenu : BaseMenu, IFramedMenu
{
    private readonly VerticalScrollBar scrollBar;
    private readonly Point scrollBarOffset;

    private Point maxOffset;
    private Point offset;

    /// <summary>Initializes a new instance of the <see cref="FramedMenu" /> class.</summary>
    /// <param name="x">The x-position of the menu.</param>
    /// <param name="y">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    /// <param name="showUpperRightCloseButton">A value indicating whether to show the right close button.</param>
    /// <param name="scrollBarOffset">An offset to the scrollbar position relative to the frame.</param>
    protected FramedMenu(
        int? x = null,
        int? y = null,
        int? width = null,
        int? height = null,
        bool showUpperRightCloseButton = false,
        Point? scrollBarOffset = null)
        : base(x, y, width, height, showUpperRightCloseButton)
    {
        this.maxOffset = new Point(-1, -1);
        this.scrollBarOffset = scrollBarOffset ?? new Point(-48, 4);
        this.scrollBar = new VerticalScrollBar(
            this,
            this.xPositionOnScreen + this.width + this.scrollBarOffset.X,
            this.yPositionOnScreen + this.scrollBarOffset.Y,
            this.height,
            () => this.offset.Y,
            value => this.offset.Y = value,
            () => 0,
            () => this.MaxOffset.Y,
            () => this.StepSize);
    }

    /// <inheritdoc />
    public override Rectangle Bounds =>
        base.Bounds with { Width = this.scrollBar.visible ? this.width - this.scrollBar.bounds.Width : this.width };

    /// <inheritdoc />
    public Point CurrentOffset => this.offset;

    /// <inheritdoc />
    public virtual Rectangle Frame =>
        base.Bounds with { Width = this.scrollBar.visible ? this.width - this.scrollBar.bounds.Width : this.width };

    /// <inheritdoc />
    public Point MaxOffset => this.maxOffset;

    /// <summary>Gets the step size for scrolling.</summary>
    public virtual int StepSize => 1;

    /// <inheritdoc />
    public override sealed void Draw(SpriteBatch spriteBatch, Point cursor) =>
        UiToolkit.DrawInFrame(spriteBatch, this.Frame, sb => this.DrawInFrame(sb, cursor));

    /// <inheritdoc />
    public virtual void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        // Draw components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent when component.visible:
                    customComponent.Draw(spriteBatch, cursor, new Point(-this.CurrentOffset.X, -this.CurrentOffset.Y));
                    if (this.Bounds.Contains(cursor)
                        && component.bounds.Contains(cursor + this.CurrentOffset)
                        && !string.IsNullOrWhiteSpace(customComponent.HoverText))
                    {
                        this.SetHoverText(customComponent.HoverText);
                    }

                    break;

                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.draw(spriteBatch);
                    if (this.Bounds.Contains(cursor)
                        && clickableTextureComponent.bounds.Contains(cursor + this.CurrentOffset)
                        && !string.IsNullOrWhiteSpace(clickableTextureComponent.hoverText))
                    {
                        this.SetHoverText(clickableTextureComponent.hoverText);
                    }

                    break;
            }
        }
    }

    /// <inheritdoc />
    public override void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        if (this.scrollBar.visible)
        {
            this.scrollBar.Draw(spriteBatch, cursor, Point.Zero);
        }

        base.DrawOver(spriteBatch, cursor);
    }

    /// <inheritdoc />
    public override ICustomMenu MoveTo(Point position)
    {
        base.MoveTo(position);
        this.scrollBar
            .MoveTo(new Point(this.xPositionOnScreen + this.width + this.scrollBarOffset.X, this.yPositionOnScreen + this.scrollBarOffset.Y));
        return this;
    }

    /// <inheritdoc />
    public override ICustomMenu ResizeTo(Point size)
    {
        base.ResizeTo(size);
        this.scrollBar
            .ResizeTo(new Point(0, size.Y))
            .MoveTo(new Point(this.Bounds.Right + this.scrollBarOffset.X, this.Bounds.Top + this.scrollBarOffset.Y));
        return this;
    }

    /// <inheritdoc />
    public IFramedMenu SetCurrentOffset(Point value)
    {
        this.offset.X = Math.Clamp(value.X, 0, this.maxOffset.X);
        this.offset.Y = Math.Clamp(value.Y, 0, this.maxOffset.Y);
        return this;
    }

    /// <inheritdoc />
    public IFramedMenu SetMaxOffset(Point value)
    {
        this.maxOffset.X = Math.Max(-1, value.X);
        this.maxOffset.Y = Math.Max(-1, value.Y);
        this.scrollBar.visible = this.maxOffset.Y > -1;
        return this;
    }

    /// <inheritdoc />
    public override bool TryHover(Point cursor)
    {
        // Hover components
        foreach (var component in this.allClickableComponents.OfType<ClickableTextureComponent>())
        {
            component.tryHover(cursor.X - this.CurrentOffset.X, cursor.Y - this.CurrentOffset.Y);
        }

        return base.TryHover(cursor);
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.scrollBar.visible && this.scrollBar.TryLeftClick(cursor))
        {
            return true;
        }

        return base.TryLeftClick(cursor + this.CurrentOffset);
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        if (this.scrollBar.visible && this.scrollBar.TryRightClick(cursor))
        {
            return true;
        }

        return base.TryRightClick(cursor + this.CurrentOffset);
    }

    /// <inheritdoc />
    public override bool TryScroll(int direction)
    {
        if (this.scrollBar.visible && this.scrollBar.TryScroll(direction))
        {
            return true;
        }

        // Scroll components
        foreach (var component in this.allClickableComponents)
        {
            if (component is ICustomComponent customComponent
                && component.visible
                && component.bounds.Contains(UiToolkit.Cursor + this.CurrentOffset)
                && customComponent.TryScroll(direction))
            {
                return true;
            }
        }

        return base.TryScroll(direction);
    }

    /// <inheritdoc />
    public override void Update(Point cursor)
    {
        base.Update(cursor);
        this.scrollBar.Update(cursor);
    }
}