namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>A component which represents an <see cref="IExpression" />.</summary>
internal abstract class ExpressionEditor : BaseComponent
{
    private static readonly Color[] Colors =
    [
        Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Pink,
    ];

    /// <summary>Initializes a new instance of the <see cref="ExpressionEditor" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="x">The x-position of the component.</param>
    /// <param name="y">The y-position of the component.</param>
    /// <param name="width">The width of the component.</param>
    /// <param name="height">The height of the component.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    public ExpressionEditor(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int x,
        int y,
        int width,
        int height,
        IExpression expression,
        int level)
        : base(inputHelper, reflectionHelper, x, y, width, height, level.ToString(CultureInfo.InvariantCulture))
    {
        this.Expression = expression;
        this.Level = level;
        this.Color = level >= 0 ? ExpressionEditor.Colors[this.Level % ExpressionEditor.Colors.Length] : Color.Black;
    }

    /// <summary>Event raised when the expression is changed.</summary>
    public abstract event EventHandler<ExpressionChangedEventArgs>? ExpressionChanged;

    public IExpression Expression { get; }

    public int Level { get; }

    protected Color Color { get; }

    protected static Color Highlighted(Color color) => Color.Lerp(color, Color.White, 0.5f);

    protected static Color Muted(Color color)
    {
        color = new Color(
            (int)Utility.Lerp(color.R, Math.Min(255, color.R + 150), 0.65f),
            (int)Utility.Lerp(color.G, Math.Min(255, color.G + 150), 0.65f),
            (int)Utility.Lerp(color.B, Math.Min(255, color.B + 150), 0.65f));

        var hsl = HslColor.FromColor(color);
        hsl.S *= 0.5f;
        return hsl.ToRgbColor();
    }

    protected void DrawComponent(
        SpriteBatch spriteBatch,
        ClickableComponent? component,
        Color color,
        Point cursor,
        Point offset)
    {
        if (component is null)
        {
            return;
        }

        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(403, 373, 9, 9),
            component.bounds.X + offset.X,
            component.bounds.Y + offset.Y,
            component.bounds.Width,
            component.bounds.Height,
            component.bounds.Contains(cursor - offset)
                ? ExpressionEditor.Highlighted(color)
                : ExpressionEditor.Muted(color),
            Game1.pixelZoom,
            false);

        if (!string.IsNullOrWhiteSpace(component.label))
        {
            spriteBatch.DrawString(
                Game1.smallFont,
                component.label,
                new Vector2(component.bounds.X + offset.X + 8, component.bounds.Y + offset.Y + 2),
                Game1.textColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);
        }
    }
}