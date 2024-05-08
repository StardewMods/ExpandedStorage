namespace StardewMods.GarbageDay.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Base menu.</summary>
internal abstract class BaseMenu : IClickableMenu
{
    /// <summary>Initializes a new instance of the <see cref="BaseMenu" /> class.</summary>
    public BaseMenu()
        : base(
            (Game1.uiViewport.Width / 2) - ((800 + (IClickableMenu.borderWidth * 2)) / 2),
            (Game1.uiViewport.Height / 2) - ((600 + (IClickableMenu.borderWidth * 2)) / 2),
            800 + (IClickableMenu.borderWidth * 2),
            600 + (IClickableMenu.borderWidth * 2),
            true) { }

    /// <inheritdoc />
    public override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
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
    }
}