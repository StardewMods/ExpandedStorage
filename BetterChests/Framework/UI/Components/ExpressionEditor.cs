namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;

/// <summary>A component which represents an <see cref="IExpression" />.</summary>
internal abstract class ExpressionEditor : BaseComponent
{
    private static readonly Color[] Colors =
    [
        Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Pink,
    ];

    private readonly ICustomComponent warningIcon;

    /// <summary>Initializes a new instance of the <see cref="ExpressionEditor" /> class.</summary>
    /// <param name="parent">The parent menu.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    protected ExpressionEditor(
        ICustomMenu? parent,
        int x,
        int y,
        int width,
        int height,
        IExpression expression,
        int level)
        : base(parent, x, y, width, height, level.ToString(CultureInfo.InvariantCulture))
    {
        this.Expression = expression;
        this.Level = level;
        this.BaseColor =
            level >= 0 ? ExpressionEditor.Colors[this.Level % ExpressionEditor.Colors.Length] : Color.Black;

        this.warningIcon = new TextureComponent(
            this.Parent,
            "warning",
            new Rectangle(x - 2, y - 7, 5, 14),
            Game1.mouseCursors,
            new Rectangle(403, 496, 5, 14),
            2f).SetHoverText(I18n.Ui_Invalid_Tooltip());
    }

    /// <summary>Event raised when the expression is changed.</summary>
    public abstract event EventHandler<ExpressionChangedEventArgs>? ExpressionChanged;

    /// <summary>Gets the expression.</summary>
    public IExpression Expression { get; }

    /// <summary>Gets the base color.</summary>
    protected Color BaseColor { get; }

    /// <summary>Gets the level.</summary>
    protected int Level { get; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        UiToolkit.DrawInFrame(
            spriteBatch,
            new Rectangle(this.bounds.X + offset.X, this.bounds.Y + offset.Y, this.bounds.Width, this.bounds.Height),
            sb => this.DrawInFrame(sb, cursor, offset));

        if (this.Expression.IsValid)
        {
            return;
        }

        this.warningIcon.Update(cursor - offset);
        this.warningIcon.Draw(spriteBatch, cursor, offset);

        if (this.warningIcon.Bounds.Contains(cursor - offset))
        {
            this.SetHoverText(this.warningIcon.HoverText);
        }
    }
}