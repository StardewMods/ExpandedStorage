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

/// <summary>Generic button component with an optional label.</summary>
internal sealed class ButtonComponent : BaseComponent
{
    /// <summary>Initializes a new instance of the <see cref="ButtonComponent" /> class.</summary>
    /// <param name="parent">The parent menu.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="name">The component name.</param>
    /// <param name="label">The component label.</param>
    public ButtonComponent(ICustomMenu? parent, int x, int y, int width, int height, string name, string label)
        : base(parent, x, y, width, height, name)
    {
        this.label = label;
        if (width == 0 && !string.IsNullOrWhiteSpace(label))
        {
            this.ResizeTo(new Point(Game1.smallFont.MeasureString(label).ToPoint().X + 20, height));
        }
    }

    /// <inheritdoc />
    public override void DrawInFrame(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(403, 373, 9, 9),
            this.bounds.X + offset.X,
            this.bounds.Y + offset.Y,
            this.bounds.Width,
            this.bounds.Height,
            this.Color,
            Game1.pixelZoom,
            false);

        if (!string.IsNullOrWhiteSpace(this.label))
        {
            spriteBatch.DrawString(
                Game1.smallFont,
                this.label,
                new Vector2(this.bounds.X + offset.X + 8, this.bounds.Y + offset.Y + 2),
                Game1.textColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);
        }
    }
}