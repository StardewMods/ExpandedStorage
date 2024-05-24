namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ExpressionTerm : ExpressionComponent
{
    private readonly ClickableComponent leftComponent;
    private readonly IExpression? leftTerm;
    private readonly ClickableTextureComponent removeButton;
    private readonly ClickableComponent rightComponent;
    private readonly IExpression? rightTerm;
    private readonly ClickableTextureComponent? warningIcon;

    private EventHandler<ExpressionChangedEventArgs>? expressionChanged;

    /// <summary>Initializes a new instance of the <see cref="ExpressionTerm" /> class.</summary>
    /// <param name="iconRegistry"></param>
    /// <param name="inputHelper"></param>
    /// <param name="reflectionHelper"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="expression"></param>
    /// <param name="level"></param>
    public ExpressionTerm(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int x,
        int y,
        int width,
        IExpression expression,
        int level)
        : base(inputHelper, reflectionHelper, x, y, width, 40, expression, level)
    {
        var subWidth = ((width - 12) / 2) - 15;

        // Left term
        this.leftTerm = expression.Expressions.ElementAtOrDefault(0);
        var text = this.leftTerm is not null ? Localized.Attribute(this.leftTerm.Term) : I18n.Attribute_Any_Name();
        this.leftComponent = new ClickableComponent(new Rectangle(x, y, subWidth, 40), "left", text);

        // Right term
        this.rightTerm = expression.Expressions.ElementAtOrDefault(1);
        this.rightComponent = new ClickableComponent(
            new Rectangle(x + subWidth + 12, y, subWidth, 40),
            "right",
            this.rightTerm?.Term ?? expression.Term);

        this.removeButton = iconRegistry
            .RequireIcon(VanillaIcon.DoNot)
            .GetComponent(IconStyle.Transparent, x + width - 24, y + 8, 2f);

        this.removeButton.name = "remove";
        this.removeButton.hoverText = I18n.Ui_Remove_Tooltip();

        if (!expression.IsValid)
        {
            this.warningIcon = new ClickableTextureComponent(
                "warning",
                new Rectangle(x - 2, y - 7, 5, 14),
                string.Empty,
                I18n.Ui_Invalid_Tooltip(),
                Game1.mouseCursors,
                new Rectangle(403, 496, 5, 14),
                2f);
        }
    }

    /// <inheritdoc />
    public override event EventHandler<ExpressionChangedEventArgs>? ExpressionChanged
    {
        add => this.expressionChanged += value;
        remove => this.expressionChanged -= value;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.leftComponent.bounds.Contains(cursor))
        {
            this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.UpdateTerm, this.Expression));

            return true;
        }

        if (this.rightComponent.bounds.Contains(cursor))
        {
            this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.UpdateTerm, this.Expression));

            return true;
        }

        if (this.removeButton.bounds.Contains(cursor))
        {
            this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.Remove, this.Expression));

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        this.DrawComponent(spriteBatch, this.leftComponent, this.Color, cursor, offset);
        this.DrawComponent(spriteBatch, this.rightComponent, this.Color, cursor, offset);
        this.removeButton.tryHover(cursor.X - offset.X, cursor.Y - offset.Y);
        this.removeButton.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);
        this.warningIcon?.tryHover(cursor.X - offset.X, cursor.Y - offset.Y);
        this.warningIcon?.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);
    }
}