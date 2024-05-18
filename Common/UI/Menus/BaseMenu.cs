namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>Base menu.</summary>
internal abstract class BaseMenu : IClickableMenu
{
    private readonly List<IClickableMenu> subMenus = [];

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
            x ?? (Game1.uiViewport.Width / 2) - ((800 + (IClickableMenu.borderWidth * 2)) / 2),
            y ?? (Game1.uiViewport.Height / 2) - ((600 + (IClickableMenu.borderWidth * 2)) / 2),
            width ?? 800 + (IClickableMenu.borderWidth * 2),
            height ?? 600 + (IClickableMenu.borderWidth * 2),
            showUpperRightCloseButton)
    {
        this.Input = inputHelper;
        this.allClickableComponents ??= [];
        if (this.upperRightCloseButton is not null)
        {
            this.allClickableComponents.Add(this.upperRightCloseButton);
        }
    }

    /// <summary>Gets or sets the hover text.</summary>
    public string? HoverText
    {
        get => this.Parent?.HoverText ?? this.hoverText;
        set
        {
            if (this.Parent is not null)
            {
                this.Parent.HoverText = value;
                return;
            }

            this.hoverText = value;
        }
    }

    /// <summary>Gets the parent menu.</summary>
    public BaseMenu? Parent { get; private set; }

    /// <summary>Gets the input helper.</summary>
    protected IInputHelper Input { get; }

    /// <summary>Gets the sub menus.</summary>
    protected IEnumerable<IClickableMenu> SubMenus => this.subMenus;

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        this.HoverText = null;

        // Draw under
        this.DrawUnder(b);

        // Draw sub-menus
        var point = Game1.getMousePosition(true);
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
                    if (clickableTextureComponent.containsPoint(point.X, point.Y)
                        && !string.IsNullOrWhiteSpace(clickableTextureComponent.hoverText))
                    {
                        this.HoverText ??= clickableTextureComponent.hoverText;
                    }

                    break;
                case ICustomComponent
                {
                    Visible: true,
                } customComponent:
                    customComponent.Draw(b);
                    if (customComponent.Contains(point.ToVector2())
                        && !string.IsNullOrWhiteSpace(customComponent.HoverText))
                    {
                        this.HoverText ??= customComponent.HoverText;
                    }

                    break;
            }
        }

        // Draw menu
        this.Draw(b);

        if (this.GetChildMenu() is not null || this.Parent?.GetChildMenu() is not null)
        {
            return;
        }

        // Draw over
        this.DrawOver(b);
    }

    /// <inheritdoc />
    public sealed override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);

        // Hold left-click sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            subMenu.leftClickHeld(x, y);
        }

        // Hold left-click components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent
                {
                    Visible: true,
                } customComponent:
                    customComponent.Update(x, y);
                    break;
            }
        }
    }

    /// <summary>Moves the menu to the specified position.</summary>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    public virtual void MoveTo(int x, int y)
    {
        var dx = x - this.xPositionOnScreen;
        var dy = y - this.yPositionOnScreen;
        this.xPositionOnScreen = x;
        this.yPositionOnScreen = y;

        // Move sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            switch (subMenu)
            {
                case BaseMenu baseMenu:
                    baseMenu.MoveTo(baseMenu.xPositionOnScreen + dx, baseMenu.yPositionOnScreen + dy);
                    break;
                default:
                    subMenu.xPositionOnScreen += dx;
                    subMenu.yPositionOnScreen += dy;
                    break;
            }
        }

        // Move components
        foreach (var component in this.allClickableComponents)
        {
            component.bounds.X += dx;
            component.bounds.Y += dy;
        }
    }

    /// <inheritdoc />
    public sealed override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

        if (this.TryHover(x, y))
        {
            return;
        }

        // Hover sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            subMenu.performHoverAction(x, y);
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
                case ICustomComponent
                {
                    Visible: true,
                } customComponent:
                    customComponent.Update(x, y);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (this.TryLeftClick(x, y))
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
                case ICustomComponent
                {
                    Visible: true,
                } customComponent when customComponent.TryLeftClick(x, y):
                    return;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);

        if (this.TryRightClick(x, y))
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
                case ICustomComponent
                {
                    Visible: true,
                } customComponent when customComponent.TryRightClick(x, y):
                    return;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        var (mouseX, mouseY) = Game1.getMousePosition(true);

        if (this.TryScroll(direction))
        {
            return;
        }

        // Scroll sub-menus
        foreach (var subMenu in this.SubMenus)
        {
            if (subMenu.isWithinBounds(mouseX, mouseY))
            {
                subMenu.receiveScrollWheelAction(direction);
            }
        }

        // Scroll components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case VerticalScrollBar
                {
                    visible: true,
                } scrollBar when scrollBar.TryScroll(direction):
                    return;
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

    /// <summary>Resize the menu to the specified dimensions.</summary>
    /// <param name="newWidth">The menu width.</param>
    /// <param name="newHeight">The menu height.</param>
    public virtual void Resize(int newWidth, int newHeight)
    {
        this.width = newWidth;
        this.height = newHeight;
    }

    /// <summary>Adds a submenu to the current menu.</summary>
    /// <param name="subMenu">The submenu to add.</param>
    protected void AddSubMenu(IClickableMenu subMenu)
    {
        this.subMenus.Add(subMenu);
        if (subMenu is BaseMenu baseMenu)
        {
            baseMenu.Parent = this;
        }
    }

    /// <summary>Draw to the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    protected virtual void Draw(SpriteBatch spriteBatch) { }

    /// <summary>Draw over the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    protected virtual void DrawOver(SpriteBatch spriteBatch)
    {
        // Draw hover text
        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, this.HoverText, string.Empty, null);
        }

        // Draw cursor
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(spriteBatch);
    }

    /// <summary>Draw under the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    protected virtual void DrawUnder(SpriteBatch spriteBatch)
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

    /// <summary>Try to perform a hover.</summary>
    /// <param name="x">The x-coordinate of the hover.</param>
    /// <param name="y">The y-coordinate of the hover.</param>
    /// <returns><c>true</c> if hover was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryHover(int x, int y) => false;

    /// <summary>Try to perform a left-click.</summary>
    /// <param name="x">The x-coordinate of the left-click.</param>
    /// <param name="y">The y-coordinate of the left-click.</param>
    /// <returns><c>true</c> if left click was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryLeftClick(int x, int y) => false;

    /// <summary>Try to perform a right-click.</summary>
    /// <param name="x">The x-coordinate of the right-click.</param>
    /// <param name="y">The y-coordinate of the right-click.</param>
    /// <returns><c>true</c> if right click was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryRightClick(int x, int y) => false;

    /// <summary>Try to perform a scroll.</summary>
    /// <param name="direction">The direction of the scroll.</param>
    /// <returns><c>true</c> if scroll was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryScroll(int direction) => false;
}