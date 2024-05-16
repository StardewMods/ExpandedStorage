namespace StardewMods.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;

// TODO: Add frame and scrolling support to BaseMenu

/// <summary>Base menu.</summary>
internal abstract class BaseMenu : IClickableMenu
{
    private readonly List<IClickableMenu> subMenus = [];

    private string? hoverText;

    /// <summary>Initializes a new instance of the <see cref="BaseMenu" /> class.</summary>
    /// <param name="x">The x-position of the menu.</param>
    /// <param name="y">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    /// <param name="showUpperRightCloseButton">A value indicating whether to show the right close button.</param>
    protected BaseMenu(
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
            showUpperRightCloseButton) =>
        this.allClickableComponents ??= [];

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

    /// <summary>Gets the sub menus.</summary>
    protected IEnumerable<IClickableMenu> SubMenus => this.subMenus;

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        this.HoverText = null;

        // Draw background
        if (!Game1.options.showClearBackgrounds && this.Parent is null && this.GetParentMenu() is null)
        {
            b.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);
        }

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
                case ClickableTextureComponent clickableTextureComponent:
                    clickableTextureComponent.draw(b);
                    if (clickableTextureComponent.containsPoint(point.X, point.Y)
                        && !string.IsNullOrWhiteSpace(clickableTextureComponent.hoverText))
                    {
                        this.HoverText ??= clickableTextureComponent.hoverText;
                    }

                    break;
                case ICustomComponent customComponent:
                    customComponent.Draw(b);
                    if (customComponent.Contains(point.ToVector2())
                        && !string.IsNullOrWhiteSpace(customComponent.HoverText))
                    {
                        this.HoverText ??= customComponent.HoverText;
                    }

                    break;
            }
        }

        this.upperRightCloseButton?.draw(b);

        // Draw menu
        this.Draw(b);

        if (this.GetChildMenu() is not null)
        {
            return;
        }

        // Draw hover text
        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(b, this.HoverText, string.Empty, null);
        }

        // Draw over
        this.DrawOver(b);

        // Draw cursor
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

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
                case ClickableTextureComponent clickableTextureComponent:
                    clickableTextureComponent.tryHover(x, y);
                    break;
                case ICustomComponent customComponent:
                    customComponent.Update(x, y);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

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
                case ICustomComponent customComponent when customComponent.TryLeftClick(x, y): return;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);

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
                case ICustomComponent customComponent when customComponent.TryRightClick(x, y): return;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction) => base.receiveScrollWheelAction(direction);

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
    protected virtual void DrawOver(SpriteBatch spriteBatch) { }

    /// <summary>Draw under the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    protected virtual void DrawUnder(SpriteBatch spriteBatch) =>
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