namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ExpressionGroup : ExpressionEditor
{
    private readonly List<ICustomComponent> components = [];
    private readonly IIconRegistry iconRegistry;
    private readonly ICustomComponent mainComponent;
    private readonly ClickableTextureComponent removeButton;

    private EventHandler<ExpressionChangedEventArgs>? expressionChanged;

    /// <summary>Initializes a new instance of the <see cref="ExpressionGroup" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="parent">The parent menu.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    public ExpressionGroup(
        IIconRegistry iconRegistry,
        ICustomMenu? parent,
        int x,
        int y,
        int width,
        IExpression expression,
        int level)
        : base(parent, x, y, width, level >= 0 ? 52 : 0, expression, level)
    {
        this.iconRegistry = iconRegistry;
        var indent = this.Level >= 0 ? 12 : 0;

        this.removeButton = iconRegistry
            .Icon(VanillaIcon.DoNot)
            .Component(IconStyle.Transparent, x + width - 36, y + 12, 2f, "remove", I18n.Ui_Remove_Tooltip());

        var toggleButton = new ButtonComponent(
            this.Parent,
            x + 8,
            y + 8,
            0,
            32,
            "toggle",
            Localized.ExpressionName(expression.ExpressionType)).SetHoverText(
            Localized.ExpressionTooltip(expression.ExpressionType));

        if (this.Level >= 0)
        {
            this.components.Add(toggleButton);
        }

        switch (expression.ExpressionType)
        {
            case ExpressionType.All or ExpressionType.Any:
                toggleButton.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
                    this,
                    new ExpressionChangedEventArgs(ExpressionChange.ToggleGroup, this.Expression));

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
                this.mainComponent = new ButtonComponent(
                    this.Parent,
                    this.bounds.X,
                    this.bounds.Y,
                    this.bounds.Width,
                    this.bounds.Height,
                    "main",
                    string.Empty);

                return;
        }

        var subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddTerm_Name()).X + 20;
        var addTerm = new ButtonComponent(
            this.Parent,
            this.bounds.X + indent,
            this.bounds.Bottom,
            subWidth,
            32,
            "addTerm",
            I18n.Ui_AddTerm_Name()).SetHoverText(I18n.Ui_AddTerm_Tooltip());

        addTerm.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.AddTerm, this.Expression));

        this.components.Add(addTerm);

        subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddGroup_Name()).X + 20;
        var addGroup = new ButtonComponent(
            this.Parent,
            addTerm.Bounds.Right + 12,
            this.bounds.Bottom,
            subWidth,
            32,
            "addGroup",
            I18n.Ui_AddGroup_Name()).SetHoverText(I18n.Ui_AddGroup_Tooltip());

        addGroup.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.AddGroup, this.Expression));

        this.components.Add(addGroup);

        if (expression.ExpressionType is not ExpressionType.Not)
        {
            subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddNot_Name()).X + 20;
            var addNot = new ButtonComponent(
                this.Parent,
                addGroup.Bounds.Right + 12,
                this.bounds.Bottom,
                subWidth,
                32,
                "addNot",
                I18n.Ui_AddNot_Name()).SetHoverText(I18n.Ui_AddNot_Tooltip());

            addNot.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.AddNot, this.Expression));

            this.components.Add(addNot);
        }

        this.bounds.Height = addTerm.Bounds.Bottom - this.bounds.Top + 12;
        this.mainComponent = new ButtonComponent(
            this.Parent,
            this.bounds.X,
            this.bounds.Y,
            this.bounds.Width,
            this.bounds.Height,
            "main",
            string.Empty);
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
            this.mainComponent.SetColor(
                this.bounds.Contains(cursor - offset) ? this.BaseColor.Highlight() : this.BaseColor.Muted());

            this.mainComponent.Draw(spriteBatch, cursor, offset);
        }

        foreach (var component in this.components)
        {
            switch (component)
            {
                case ExpressionEditor:
                    component.Draw(spriteBatch, cursor, offset);

                    if (component.Bounds.Contains(cursor - offset))
                    {
                        this.SetHoverText(component.HoverText);
                    }

                    break;

                case ButtonComponent buttonComponent:
                    component.SetColor(
                        buttonComponent.bounds.Contains(cursor - offset) ? Color.Gray.Highlight() : Color.Gray.Muted());

                    component.Draw(spriteBatch, cursor, offset);

                    if (component.Bounds.Contains(cursor - offset))
                    {
                        this.SetHoverText(component.HoverText);
                    }

                    break;
            }
        }

        if (this.Level < 0)
        {
            return;
        }

        this.removeButton?.tryHover(cursor.X - offset.X, cursor.Y - offset.Y);
        this.removeButton?.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);

        if (this.removeButton?.bounds.Contains(cursor - offset) == true)
        {
            this.SetHoverText(this.removeButton.hoverText);
        }
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.components.Any(component => component.TryLeftClick(cursor)))
        {
            return true;
        }

        if (this.Level < 0)
        {
            return false;
        }

        if (this.removeButton?.bounds.Contains(cursor) != true)
        {
            return false;
        }

        this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.Remove, this.Expression));

        return true;
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
                    this.Parent,
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
                    this.Parent,
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