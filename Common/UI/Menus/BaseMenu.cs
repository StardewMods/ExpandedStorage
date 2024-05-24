#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Base menu.</summary>
internal abstract class BaseMenu : IClickableMenu, ICustomMenu
{
    private readonly List<IClickableMenu> subMenus = [];

    private Rectangle bounds;
    private string? hoverText;

    /// <summary>Initializes a new instance of the <see cref="BaseMenu" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="x">The x-position of the menu.</param>
    /// <param name="y">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    /// <param name="showUpperRightCloseButton">A value indicating whether to show the right close button.</param>
    protected BaseMenu(
        IInputHelper inputHelper,
        int? x = null,
        int? y = null,
        int? width = null,
        int? height = null,
        bool showUpperRightCloseButton = false)
        : base(
            x ?? (Game1.uiViewport.Width / 2) - ((width ?? 800 + (IClickableMenu.borderWidth * 2)) / 2),
            y ?? (Game1.uiViewport.Height / 2) - ((height ?? 600 + (IClickableMenu.borderWidth * 2)) / 2),
            width ?? 800 + (IClickableMenu.borderWidth * 2),
            height ?? 600 + (IClickableMenu.borderWidth * 2),
            showUpperRightCloseButton)
    {
        this.Input = inputHelper;
        this.allClickableComponents ??= [];
        this.bounds = new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        if (this.upperRightCloseButton is not null)
        {
            this.allClickableComponents.Add(this.upperRightCloseButton);
        }
    }

    /// <inheritdoc />
    public Rectangle Bounds => this.bounds;

    /// <inheritdoc />
    public Point Dimensions => this.Bounds.Location;

    /// <inheritdoc />
    public string? HoverText => (this.Parent as ICustomMenu)?.HoverText ?? this.hoverText;

    /// <inheritdoc />
    public IEnumerable<IClickableMenu> SubMenus => this.subMenus;

    /// <inheritdoc />
    public IClickableMenu? Parent { get; private set; }

    /// <summary>Gets the input helper.</summary>
    protected IInputHelper Input { get; }

    /// <inheritdoc />
    public ICustomMenu AddSubMenu(IClickableMenu subMenu)
    {
        this.subMenus.Add(subMenu);
        if (subMenu is BaseMenu baseMenu)
        {
            baseMenu.Parent = this;
        }

        return this;
    }

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        var cursor = this.Input.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        this.SetHoverText(null);

        // Draw under
        this.DrawUnder(b, cursor);

        // Draw sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            subMenu.draw(b, red, green, blue);
        }

        // Draw components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.draw(b);
                    if (clickableTextureComponent.bounds.Contains(cursor)
                        && !string.IsNullOrWhiteSpace(clickableTextureComponent.hoverText))
                    {
                        this.SetHoverText(clickableTextureComponent.hoverText);
                    }

                    break;
                case ICustomComponent customComponent when component.visible:
                    customComponent.Draw(b, cursor, Point.Zero);
                    if (component.bounds.Contains(cursor) && !string.IsNullOrWhiteSpace(customComponent.HoverText))
                    {
                        this.SetHoverText(customComponent.HoverText);
                    }

                    break;
            }
        }

        // Draw menu
        this.Draw(b, cursor);

        if (this.GetChildMenu() is not null || this.Parent?.GetChildMenu() is not null)
        {
            return;
        }

        // Draw over
        this.DrawOver(b, cursor);
    }

    /// <inheritdoc />
    public virtual void Draw(SpriteBatch spriteBatch, Point cursor) { }

    /// <inheritdoc />
    public virtual void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        // Draw hover text
        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, this.HoverText, null, null);
        }

        // Draw cursor
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(spriteBatch);
    }

    /// <inheritdoc />
    public virtual void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        // Draw background
        if (!Game1.options.showClearBackgrounds && this.Parent is null && this.GetParentMenu() is null)
        {
            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);
        }

        Game1.drawDialogueBox(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            false,
            true,
            null,
            false,
            false);
    }

    /// <inheritdoc />
    public sealed override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        var cursor = new Point(x, y);

        // Hold left-click sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            switch (subMenu)
            {
                case BaseMenu baseMenu:
                    baseMenu.Update(cursor);
                    baseMenu.leftClickHeld(x, y);
                    break;
                default:
                    subMenu.leftClickHeld(x, y);
                    break;
            }
        }

        // Hold left-click components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent when component.visible:
                    customComponent.Update(cursor);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public virtual ICustomMenu MoveTo(Point position)
    {
        var delta = position - this.Position.ToPoint();

        // Move sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            switch (subMenu)
            {
                case BaseMenu baseMenu:
                    baseMenu.MoveTo(baseMenu.Position.ToPoint() + delta);
                    break;
                default:
                    subMenu.xPositionOnScreen += delta.X;
                    subMenu.yPositionOnScreen += delta.Y;
                    break;
            }
        }

        // Move components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent:
                    customComponent.MoveTo(customComponent.Position + delta);
                    break;
                default:
                    component.bounds.Offset(delta);
                    break;
            }
        }

        this.bounds.Location = position;
        this.xPositionOnScreen = position.X;
        this.yPositionOnScreen = position.Y;
        return this;
    }

    /// <inheritdoc />
    public sealed override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        var cursor = new Point(x, y);

        if (this.TryHover(cursor))
        {
            return;
        }

        // Hover sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            switch (subMenu)
            {
                case BaseMenu baseMenu when baseMenu.TryHover(cursor): return;
                case BaseMenu baseMenu:
                    baseMenu.Update(cursor);
                    baseMenu.performHoverAction(x, y);
                    break;
                default:
                    subMenu.performHoverAction(x, y);
                    break;
            }
        }

        // Hover components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.tryHover(x, y);
                    break;
                case ICustomComponent customComponent when component.visible:
                    customComponent.Update(cursor);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        var cursor = new Point(x, y);

        if (this.TryLeftClick(cursor))
        {
            return;
        }

        // Click sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            subMenu.receiveLeftClick(x, y, playSound);
        }

        // Click components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent when component.visible
                    && component.bounds.Contains(cursor)
                    && customComponent.TryLeftClick(cursor):
                    return;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
        var cursor = new Point(x, y);

        if (this.TryRightClick(cursor))
        {
            return;
        }

        // Click sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            subMenu.receiveRightClick(x, y, playSound);
        }

        // Click components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent when component.visible
                    && component.bounds.Contains(cursor)
                    && customComponent.TryRightClick(cursor):
                    return;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        var cursor = this.Input.GetCursorPosition().GetScaledScreenPixels();
        if (this.Bounds.Contains(cursor) && this.TryScroll(direction))
        {
            return;
        }

        // Scroll sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            switch (subMenu)
            {
                case BaseMenu baseMenu when baseMenu.Bounds.Contains(cursor) && baseMenu.TryScroll(direction): return;
                default:
                    subMenu.receiveScrollWheelAction(direction);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);

        // Un-click sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            subMenu.releaseLeftClick(x, y);
        }
    }

    /// <inheritdoc />
    public virtual ICustomMenu ResizeTo(Point size)
    {
        this.bounds.Size = size;
        return this;
    }

    /// <inheritdoc />
    public ICustomMenu SetHoverText(string? value)
    {
        if (this.Parent is ICustomMenu parent)
        {
            parent.SetHoverText(value);
            return this;
        }

        this.hoverText = value;
        return this;
    }

    /// <inheritdoc />
    public virtual bool TryHover(Point cursor) => false;

    /// <inheritdoc />
    public virtual bool TryLeftClick(Point cursor) => false;

    /// <inheritdoc />
    public virtual bool TryRightClick(Point cursor) => false;

    /// <inheritdoc />
    public virtual bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public virtual void Update(Point cursor)
    {
        // Do nothing
    }
}