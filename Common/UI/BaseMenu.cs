namespace StardewMods.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;

/// <summary>Base menu.</summary>
internal abstract class BaseMenu : IClickableMenu
{
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
        bool showUpperRightCloseButton = true)
        : base(
            x ?? (Game1.uiViewport.Width / 2) - ((800 + (IClickableMenu.borderWidth * 2)) / 2),
            y ?? (Game1.uiViewport.Height / 2) - ((600 + (IClickableMenu.borderWidth * 2)) / 2),
            width ?? 800 + (IClickableMenu.borderWidth * 2),
            height ?? 600 + (IClickableMenu.borderWidth * 2),
            showUpperRightCloseButton) =>
        this.allClickableComponents ??= [];

    /// <summary>Gets or sets the hover text.</summary>
    public string? HoverText { get; set; }

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        this.HoverText = null;

        // Draw background
        if (!Game1.options.showClearBackgrounds)
        {
            b.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);
        }

        // Draw menu background
        Game1.drawDialogueBox(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            false,
            true,
            null,
            false,
            false,
            red,
            green,
            blue);

        this.upperRightCloseButton?.draw(b);

        // Draw components
        var point = Game1.getMousePosition(true);
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

        // Draw cursor
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
    }

    /// <summary>Draw to the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    public abstract void Draw(SpriteBatch spriteBatch);

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

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

        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent when customComponent.TryRightClick(x, y): return;
            }
        }
    }
}