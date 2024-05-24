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
internal sealed class ExpressionGroup : ExpressionEditor
{
    private readonly ClickableComponent? addGroup;
    private readonly ClickableComponent? addNot;
    private readonly ClickableComponent? addTerm;
    private readonly List<ExpressionEditor> components = [];
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly IReflectionHelper reflectionHelper;
    private readonly ClickableTextureComponent? removeButton;
    private readonly ClickableComponent? toggleButton;
    private readonly ClickableTextureComponent? warningIcon;

    private EventHandler<ExpressionChangedEventArgs>? expressionChanged;

    /// <summary>Initializes a new instance of the <see cref="ExpressionGroup" /> class.</summary>
    /// <param name="iconRegistry"></param>
    /// <param name="inputHelper"></param>
    /// <param name="reflectionHelper"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="expression"></param>
    /// <param name="level"></param>
    public ExpressionGroup(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int x,
        int y,
        int width,
        IExpression expression,
        int level)
        : base(inputHelper, reflectionHelper, x, y, width, level >= 0 ? 52 : 0, expression, level)
    {
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.reflectionHelper = reflectionHelper;
        var indent = this.Level >= 0 ? 12 : 0;
        var text = Localized.ExpressionName(expression.ExpressionType);
        var tooltip = Localized.ExpressionTooltip(expression.ExpressionType);

        if (this.Level >= 0)
        {
            this.toggleButton = new ClickableComponent(
                new Rectangle(x + 8, y + 8, Game1.smallFont.MeasureString(text).ToPoint().X + 20, 32),
                "toggle",
                text);

            this.removeButton = iconRegistry
                .Icon(VanillaIcon.DoNot)
                .Component(IconStyle.Transparent, x + width - 36, y + 12, 2f);

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

        switch (expression.ExpressionType)
        {
            case ExpressionType.All or ExpressionType.Any:
                foreach (var subExpression in expression.Expressions)
                {
                    this.AddSubExpression(subExpression);
                }

                break;

            case ExpressionType.Not:
                var innerExpression = expression.Expressions.ElementAtOrDefault(0);
                if (innerExpression is null)
                {
                    break;
                }

                this.AddSubExpression(innerExpression);
                return;
        }

        var subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddTerm_Name()).X + 20;
        this.addTerm = new ClickableComponent(
            new Rectangle(this.bounds.X + indent, this.bounds.Bottom, subWidth, 32),
            "addTerm",
            I18n.Ui_AddTerm_Name());

        subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddGroup_Name()).X + 20;
        this.addGroup = new ClickableComponent(
            new Rectangle(this.addTerm.bounds.Right + 12, this.bounds.Bottom, subWidth, 32),
            "addGroup",
            I18n.Ui_AddGroup_Name());

        if (expression.ExpressionType is ExpressionType.Not)
        {
            this.bounds.Height = this.addTerm.bounds.Bottom - this.bounds.Top + 12;
            return;
        }

        subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddNot_Name()).X + 20;
        this.addNot = new ClickableComponent(
            new Rectangle(this.addGroup.bounds.Right + 12, this.bounds.Bottom, subWidth, 32),
            "addNot",
            I18n.Ui_AddNot_Name());

        this.bounds.Height = this.addTerm.bounds.Bottom - this.bounds.Top + 12;
    }

    /// <inheritdoc />
    public override event EventHandler<ExpressionChangedEventArgs>? ExpressionChanged
    {
        add => this.expressionChanged += value;
        remove => this.expressionChanged -= value;
    }

    /// <inheritdoc />
    public override void DrawInFrame(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        if (this.Level >= 0)
        {
            this.DrawComponent(spriteBatch, this, this.Color, cursor, offset);
        }

        foreach (var component in this.components)
        {
            component.Draw(spriteBatch, cursor, offset);
        }

        this.DrawComponent(spriteBatch, this.toggleButton, Color.Gray, cursor, offset);
        this.DrawComponent(spriteBatch, this.addGroup, Color.Gray, cursor, offset);
        this.DrawComponent(spriteBatch, this.addTerm, Color.Gray, cursor, offset);
        this.DrawComponent(spriteBatch, this.addNot, Color.Gray, cursor, offset);
        this.removeButton?.tryHover(cursor.X - offset.X, cursor.Y - offset.Y);
        this.removeButton?.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);
        this.warningIcon?.tryHover(cursor.X - offset.X, cursor.Y - offset.Y);
        this.warningIcon?.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.components.Any(component => component.TryLeftClick(cursor)))
        {
            return true;
        }

        if (this.removeButton?.bounds.Contains(cursor) == true)
        {
            this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.Remove, this.Expression));

            return true;
        }

        if (this.toggleButton?.bounds.Contains(cursor) == true
            && this.Expression.ExpressionType is ExpressionType.All or ExpressionType.Any)
        {
            this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.ToggleGroup, this.Expression));

            return true;
        }

        return false;
    }

    private void AddSubExpression(IExpression expression)
    {
        var indent = this.Level >= 0 ? 12 : 0;
        ExpressionEditor editor;
        switch (expression.ExpressionType)
        {
            case ExpressionType.All or ExpressionType.Any or ExpressionType.Not:
                var expressionGroup = new ExpressionGroup(
                    this.iconRegistry,
                    this.inputHelper,
                    this.reflectionHelper,
                    this.bounds.X + indent,
                    this.bounds.Bottom,
                    this.bounds.Width - (indent * 2),
                    expression,
                    this.Level + 1);

                editor = expressionGroup;
                break;
            default:
                var expressionTerm = new ExpressionTerm(
                    this.iconRegistry,
                    this.inputHelper,
                    this.reflectionHelper,
                    this.bounds.X + indent,
                    this.bounds.Bottom,
                    this.bounds.Width - (indent * 2),
                    expression,
                    this.Level + 1);

                editor = expressionTerm;
                break;
        }

        editor.ExpressionChanged += this.OnExpressionChanged;
        this.bounds.Height = editor.bounds.Bottom - this.bounds.Top + 12;
        this.components.Add(editor);
    }

    private void OnExpressionChanged(object? sender, ExpressionChangedEventArgs e) =>
        this.expressionChanged?.InvokeAll(sender, e);
}