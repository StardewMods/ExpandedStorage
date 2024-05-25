namespace StardewMods.BetterChests.Framework.UI.Menus;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>A sub-menu for editing the expression tree.</summary>
internal sealed class ExpressionEditor : FramedMenu
{
    private static readonly Color[] Colors =
    [
        Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Pink,
    ];

    private readonly IExpressionHandler expressionHandler;
    private readonly Func<string> getSearchText;
    private readonly IIconRegistry iconRegistry;

    private readonly List<(Color Color, ClickableComponent Component, IExpression? Expression, string Tooltip, Action?
        Action)> items = [];

    private readonly Action<string> setSearchText;

    private IExpression? baseExpression;

    /// <summary>Initializes a new instance of the <see cref="ExpressionEditor" /> class.</summary>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="getSearchText">A function that gets the current search text.</param>
    /// <param name="setSearchText">An action that sets the current search text.</param>
    /// <param name="xPosition">The x-position of the menu.</param>
    /// <param name="yPosition">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    public ExpressionEditor(
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        Func<string> getSearchText,
        Action<string> setSearchText,
        int xPosition,
        int yPosition,
        int width,
        int height)
        : base(xPosition, yPosition, width, height)
    {
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
        this.getSearchText = getSearchText;
        this.setSearchText = setSearchText;
    }

    /// <inheritdoc />
    public override Rectangle Frame =>
        new(this.xPositionOnScreen - 4, this.yPositionOnScreen - 8, this.width + 8, this.height + 16);

    /// <inheritdoc />
    public override int StepSize => 40;

    private string SearchText
    {
        get => this.getSearchText();
        set => this.setSearchText(value);
    }

    /// <inheritdoc />
    public override void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        foreach (var (baseColor, component, expression, tooltip, _) in this.items)
        {
            var hover = component.bounds.Contains(cursor - this.CurrentOffset);
            var color = hover && this.Parent!.GetChildMenu() is null
                ? ExpressionEditor.Highlighted(baseColor)
                : ExpressionEditor.Muted(baseColor);

            switch (expression?.ExpressionType)
            {
                case ExpressionType.All:
                case ExpressionType.Any:
                case ExpressionType.Not:
                    IClickableMenu.drawTextureBox(
                        spriteBatch,
                        Game1.mouseCursors,
                        new Rectangle(403, 373, 9, 9),
                        component.bounds.X - this.CurrentOffset.X,
                        component.bounds.Y - this.CurrentOffset.Y,
                        component.bounds.Width,
                        component.bounds.Height,
                        color,
                        Game1.pixelZoom,
                        false);

                    continue;
                case ExpressionType.Comparable: continue;
            }

            if (component is ClickableTextureComponent clickableTextureComponent)
            {
                if (hover)
                {
                    this.SetHoverText(clickableTextureComponent.hoverText);
                }

                clickableTextureComponent.draw(
                    spriteBatch,
                    Color.White,
                    1f,
                    0,
                    -this.CurrentOffset.X,
                    -this.CurrentOffset.Y);

                continue;
            }

            if (hover && !string.IsNullOrWhiteSpace(tooltip))
            {
                this.SetHoverText(tooltip);
            }

            if (component.label is not null)
            {
                IClickableMenu.drawTextureBox(
                    spriteBatch,
                    Game1.mouseCursors,
                    new Rectangle(432, 439, 9, 9),
                    component.bounds.X - this.CurrentOffset.X,
                    component.bounds.Y - this.CurrentOffset.Y,
                    component.bounds.Width,
                    component.bounds.Height,
                    color,
                    Game1.pixelZoom,
                    false);
            }

            if (string.IsNullOrWhiteSpace(component.label))
            {
                continue;
            }

            spriteBatch.DrawString(
                Game1.smallFont,
                component.label,
                new Vector2(
                    component.bounds.X - this.CurrentOffset.X + 8,
                    component.bounds.Y - this.CurrentOffset.Y + 2),
                Game1.textColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);
        }
    }

    /// <inheritdoc />
    public override void DrawUnder(SpriteBatch b, Point cursor) { }

    /// <summary>Re-initializes the components of the object with the given initialization expression.</summary>
    /// <param name="initExpression">The initial expression, or null to clear.</param>
    public void ReInitializeComponents(IExpression? initExpression)
    {
        const int lineHeight = 40;
        const int tabWidth = 12;

        this.baseExpression = initExpression;
        this.items.Clear();
        var currentX = this.xPositionOnScreen;
        var currentY = this.yPositionOnScreen;

        if (initExpression is null)
        {
            return;
        }

        var offsetX = -1;
        Enqueue(initExpression);
        this
            .SetMaxOffset(new Point(-1, Math.Max(0, currentY - this.yPositionOnScreen - this.height)))
            .SetCurrentOffset(new Point(0, Math.Min(Math.Max(0, this.CurrentOffset.Y), this.MaxOffset.Y)));

        return;

        void AddGroup(IExpression expression)
        {
            if (offsetX == -1)
            {
                offsetX = 0;
                foreach (var subExpression in expression.Expressions)
                {
                    Enqueue(subExpression);
                }

                AddInsert(expression);
                return;
            }

            // Parent component
            var initialY = currentY;
            var index = this.items.Count;
            var color = ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length];
            var component = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, this.width - (offsetX * 2), lineHeight),
                index.ToString(CultureInfo.InvariantCulture));

            this.items.Add((color, component, expression, string.Empty, null));

            var text = Localized.ExpressionName(expression.ExpressionType);
            var tooltip = Localized.ExpressionTooltip(expression.ExpressionType);

            Action? action = expression.ExpressionType switch
            {
                ExpressionType.All or ExpressionType.Any => () => this.ToggleGroup(expression), _ => null,
            };

            var toggleGroup = new ClickableComponent(
                new Rectangle(
                    component.bounds.X + 8,
                    component.bounds.Y + 8,
                    (int)Game1.smallFont.MeasureString(text).X + 20,
                    32),
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                text);

            this.items.Add((Color.Gray, toggleGroup, null, tooltip, action));

            AddRemove(expression, component);

            currentY += lineHeight + tabWidth;
            offsetX += tabWidth;

            switch (expression.ExpressionType)
            {
                case ExpressionType.All or ExpressionType.Any:
                    foreach (var subExpression in expression.Expressions)
                    {
                        Enqueue(subExpression);
                    }

                    AddInsert(expression);
                    break;

                case ExpressionType.Not:
                    var innerExpression = expression.Expressions.ElementAtOrDefault(0);
                    if (innerExpression is null)
                    {
                        AddInsert(expression);
                    }
                    else
                    {
                        Enqueue(innerExpression);
                    }

                    break;
            }

            offsetX -= tabWidth;

            if (!expression.IsValid)
            {
                AddWarning(component);
            }

            var newHeight = currentY - initialY - tabWidth;
            if (component.bounds.Height == newHeight)
            {
                return;
            }

            component.bounds.Height = newHeight + tabWidth;
            currentY += tabWidth;
        }

        void AddTerm(IExpression expression)
        {
            // Parent component
            var index = this.items.Count;
            var component = new ClickableComponent(
                new Rectangle(
                    currentX + offsetX - tabWidth,
                    currentY - 4,
                    this.width - (offsetX * 2) + (tabWidth * 2),
                    lineHeight + 8),
                index.ToString(CultureInfo.InvariantCulture));

            this.items.Add((Color.White, component, expression, string.Empty, null));

            var subWidth = ((this.width - tabWidth) / 2) - offsetX - 15;

            // Left component
            var leftTerm = expression.Expressions.ElementAtOrDefault(0);
            var text = leftTerm is not null ? Localized.Attribute(leftTerm.Term) : I18n.Attribute_Any_Name();

            var color = ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length];
            var leftComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, lineHeight),
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                text);

            this.items.Add(
                (color, leftComponent, leftTerm, string.Empty, () => this.ShowDropdown(expression, leftComponent)));

            // Right component
            var rightTerm = expression.Expressions.ElementAtOrDefault(1);
            var rightComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX + subWidth + tabWidth, currentY, subWidth, lineHeight),
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                rightTerm?.Term ?? expression.Term);

            this.items.Add(
                (color, rightComponent, rightTerm, string.Empty, () => this.ShowPopup(expression, rightComponent)));

            AddRemove(expression, component);

            if (!expression.IsValid)
            {
                AddWarning(component);
            }

            currentY += lineHeight + tabWidth;
        }

        void AddInsert(IExpression expression)
        {
            var initialX = offsetX;
            var subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddTerm_Name()).X + 20;
            var addTerm = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, 32),
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                I18n.Ui_AddTerm_Name());

            this.items.Add(
                (Color.Gray, addTerm, null, I18n.Ui_AddTerm_Tooltip(),
                    () => this.Add(expression, ExpressionType.Comparable)));

            offsetX += subWidth + tabWidth;
            subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddGroup_Name()).X + 20;
            var addGroup = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, 32),
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                I18n.Ui_AddGroup_Name());

            this.items.Add(
                (Color.Gray, addGroup, null, I18n.Ui_AddGroup_Tooltip(),
                    () => this.Add(expression, ExpressionType.All)));

            if (expression.ExpressionType is ExpressionType.Not)
            {
                offsetX = initialX;
                currentY += lineHeight + tabWidth;
                return;
            }

            offsetX += subWidth + tabWidth;
            subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddNot_Name()).X + 20;
            var addNot = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, 32),
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                I18n.Ui_AddNot_Name());

            this.items.Add(
                (Color.Gray, addNot, null, I18n.Ui_AddNot_Tooltip(), () => this.Add(expression, ExpressionType.Not)));

            offsetX = initialX;
            currentY += lineHeight + tabWidth;
        }

        void AddRemove(IExpression expression, ClickableComponent component)
        {
            var removeComponent = new ClickableTextureComponent(
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                new Rectangle(component.bounds.Right - 36, component.bounds.Y + 12, 24, 24),
                string.Empty,
                I18n.Ui_Remove_Tooltip(),
                Game1.mouseCursors,
                new Rectangle(322, 498, 12, 12),
                2f);

            this.items.Add(
                (Color.White, removeComponent, null, I18n.Ui_Remove_Tooltip(), () => this.Remove(expression)));
        }

        void AddWarning(ClickableComponent component)
        {
            var subComponent = new ClickableTextureComponent(
                this.items.Count.ToString(CultureInfo.InvariantCulture),
                new Rectangle(component.bounds.X - 2, component.bounds.Y - 7, 5, 14),
                string.Empty,
                I18n.Ui_Invalid_Tooltip(),
                Game1.mouseCursors,
                new Rectangle(403, 496, 5, 14),
                2f);

            this.items.Add((Color.White, subComponent, null, I18n.Ui_Invalid_Tooltip(), null));
        }

        void Enqueue(IExpression expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionType.All or ExpressionType.Any or ExpressionType.Not:
                    AddGroup(expression);
                    break;

                default:
                    AddTerm(expression);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public override bool TryHover(Point cursor)
    {
        if (base.TryHover(cursor))
        {
            return true;
        }

        foreach (var (_, component, _, _, _) in this.items)
        {
            if (component is ClickableTextureComponent clickableTextureComponent)
            {
                clickableTextureComponent.tryHover(cursor.X + this.CurrentOffset.X, cursor.Y + this.CurrentOffset.Y);
            }
        }

        return false;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (base.TryLeftClick(cursor))
        {
            return true;
        }

        foreach (var (_, component, _, _, action) in this.items)
        {
            if (action is null || !component.bounds.Contains(cursor + this.CurrentOffset))
            {
                continue;
            }

            action.Invoke();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        if (base.TryRightClick(cursor))
        {
            return true;
        }

        foreach (var (_, component, _, _, action) in this.items)
        {
            if (!component.bounds.Contains(cursor + this.CurrentOffset))
            {
                continue;
            }

            action?.Invoke();
            return true;
        }

        return false;
    }

    private static Color Highlighted(Color color) => Color.Lerp(color, Color.White, 0.5f);

    private static Color Muted(Color color)
    {
        color = new Color(
            (int)Utility.Lerp(color.R, Math.Min(255, color.R + 150), 0.65f),
            (int)Utility.Lerp(color.G, Math.Min(255, color.G + 150), 0.65f),
            (int)Utility.Lerp(color.B, Math.Min(255, color.B + 150), 0.65f));

        var hsl = HslColor.FromColor(color);
        hsl.S *= 0.5f;
        return hsl.ToRgbColor();
    }

    private void Add(IExpression toAddTo, ExpressionType expressionType)
    {
        if (this.baseExpression is null
            || !this.expressionHandler.TryCreateExpression(expressionType, out var newChild))
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toAddTo.Expressions = newChildren;
        this.SearchText = this.baseExpression.Text;
        this.ReInitializeComponents(this.baseExpression);
        return;

        IEnumerable<IExpression> GetChildren()
        {
            foreach (var child in toAddTo.Expressions)
            {
                yield return child;
            }

            yield return newChild;
        }
    }

    private void ChangeAttribute(IExpression toChange, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !ItemAttributeExtensions.TryParse(value, out var attribute))
        {
            return;
        }

        var dynamicTerm = toChange.Expressions.ElementAtOrDefault(0);
        if (this.baseExpression is null || dynamicTerm?.ExpressionType is not ExpressionType.Dynamic)
        {
            return;
        }

        dynamicTerm.Term = attribute.ToStringFast();
        this.SearchText = this.baseExpression.Text;
        this.ReInitializeComponents(this.baseExpression);
    }

    private void ChangeTerm(IExpression toChange, string term)
    {
        var staticTerm = toChange.Expressions.ElementAtOrDefault(1);
        if (this.baseExpression is null
            || staticTerm?.ExpressionType is not (ExpressionType.Quoted or ExpressionType.Static))
        {
            return;
        }

        staticTerm.Term = term;
        this.SearchText = this.baseExpression.Text;
        this.ReInitializeComponents(this.baseExpression);
    }

    private void Remove(IExpression toRemove)
    {
        if (this.baseExpression is null || toRemove.Parent is null)
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toRemove.Parent.Expressions = newChildren;
        this.SearchText = this.baseExpression.Text;
        this.ReInitializeComponents(this.baseExpression);
        return;

        IEnumerable<IExpression> GetChildren()
        {
            foreach (var child in toRemove.Parent.Expressions)
            {
                if (child != toRemove)
                {
                    yield return child;
                }
            }
        }
    }

    private void ShowDropdown(IExpression expression, ClickableComponent component)
    {
        var dropdown = new Dropdown<ItemAttribute>(
            component,
            ItemAttributeExtensions.GetValues().AsEnumerable(),
            static attribute => Localized.Attribute(attribute.ToStringFast()));

        dropdown.OptionSelected += (_, attribute) => this.ChangeAttribute(expression, attribute.ToStringFast());
        this.Parent?.SetChildMenu(dropdown);
    }

    private void ShowPopup(IExpression expression, ClickableComponent component)
    {
        var leftTerm = expression.Expressions.ElementAtOrDefault(0);
        if (leftTerm?.ExpressionType is not ExpressionType.Dynamic
            || !ItemAttributeExtensions.TryParse(leftTerm.Term, out var itemAttribute))
        {
            return;
        }

        var popupItems = itemAttribute switch
        {
            ItemAttribute.Any => ItemRepository.Categories.Concat(ItemRepository.Names).Concat(ItemRepository.Tags),
            ItemAttribute.Category => ItemRepository.Categories,
            ItemAttribute.Name => ItemRepository.Names,
            ItemAttribute.Quality => ItemQualityExtensions.GetNames(),
            ItemAttribute.Quantity =>
                Enumerable.Range(0, 999).Select(i => i.ToString(CultureInfo.InvariantCulture)),
            ItemAttribute.Tags => ItemRepository.Tags,
            _ => throw new ArgumentOutOfRangeException(nameof(expression)),
        };

        var popupSelect = new PopupSelect<string>(this.iconRegistry, popupItems, component.label, maxItems: 10);

        popupSelect.OptionSelected += (_, _) =>
        {
            this.ChangeTerm(expression, popupSelect.CurrentText);
        };

        this.Parent?.SetChildMenu(popupSelect);
    }

    private void ToggleGroup(IExpression toToggle)
    {
        var expressionType = toToggle.ExpressionType switch
        {
            ExpressionType.All => ExpressionType.Any,
            ExpressionType.Any => ExpressionType.All,
            _ => ExpressionType.All,
        };

        if (this.baseExpression is null
            || toToggle.Parent is null
            || !this.expressionHandler.TryCreateExpression(expressionType, out var newChild))
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toToggle.Parent.Expressions = newChildren;
        this.SearchText = this.baseExpression.Text;
        this.ReInitializeComponents(this.baseExpression);
        return;

        IEnumerable<IExpression> GetChildren()
        {
            foreach (var child in toToggle.Parent.Expressions)
            {
                if (child != toToggle)
                {
                    yield return child;

                    continue;
                }

                newChild.Expressions = child.Expressions;
                yield return newChild;
            }
        }
    }
}