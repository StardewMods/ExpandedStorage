namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Enums;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewValley.Menus;

/// <summary>A sub-menu for editing the expression tree.</summary>
internal sealed class ExpressionEditor : BaseMenu
{
    private static readonly Color[] Colors =
    [
        Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Pink,
    ];

    private readonly IExpressionHandler expressionHandler;

    private readonly List<(Color Color, ClickableComponent Component, IExpression? Expression, string Tooltip, Action?
        Action)> items = [];

    private readonly SearchMenu parent;

    private IExpression? baseExpression;
    private int offsetY;

    /// <summary>Initializes a new instance of the <see cref="ExpressionEditor" /> class.</summary>
    /// <param name="parent">The parent search menu.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="xPosition">The x-position of the menu.</param>
    /// <param name="yPosition">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    public ExpressionEditor(
        SearchMenu parent,
        IExpressionHandler expressionHandler,
        int xPosition,
        int yPosition,
        int width,
        int height)
        : base(xPosition, yPosition, width, height, false)
    {
        this.parent = parent;
        this.expressionHandler = expressionHandler;
    }

    /// <summary>Gets the maximum offset.</summary>
    public int MaxOffset { get; private set; }

    /// <summary>Gets or sets the y-offset.</summary>
    public int OffsetY
    {
        get => this.offsetY;
        set => this.offsetY = Math.Min(this.MaxOffset, Math.Max(0, value));
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        foreach (var (baseColor, component, expression, tooltip, _) in this.items)
        {
            var hover = component.containsPoint(mouseX, mouseY - this.OffsetY);
            var color = hover && this.parent.DropDown is null && this.parent.GetChildMenu() is null
                ? ExpressionEditor.Highlighted(baseColor)
                : ExpressionEditor.Muted(baseColor);

            switch (expression?.ExpressionType)
            {
                case ExpressionType.All:
                case ExpressionType.Any:
                case ExpressionType.Not:
                    IClickableMenu.drawTextureBox(
                        b,
                        Game1.mouseCursors,
                        new Rectangle(403, 373, 9, 9),
                        component.bounds.X,
                        component.bounds.Y - this.OffsetY,
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
                    this.parent.HoverText ??= clickableTextureComponent.hoverText;
                }

                clickableTextureComponent.draw(b, Color.White, 1f, yOffset: -this.OffsetY);
                continue;
            }

            if (hover && !string.IsNullOrWhiteSpace(tooltip))
            {
                this.parent.HoverText ??= tooltip;
            }

            if (component.label is not null)
            {
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.mouseCursors,
                    new Rectangle(432, 439, 9, 9),
                    component.bounds.X,
                    component.bounds.Y - this.OffsetY,
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

            b.DrawString(
                Game1.smallFont,
                component.label,
                new Vector2(component.bounds.X + 8, component.bounds.Y - this.OffsetY + 2),
                Game1.textColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);
        }
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y + this.OffsetY);
        foreach (var (_, component, _, _, _) in this.items)
        {
            if (component is ClickableTextureComponent clickableTextureComponent)
            {
                clickableTextureComponent.tryHover(x, y + this.OffsetY);
            }
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        foreach (var (_, component, _, _, action) in this.items)
        {
            if (action is not null && component.containsPoint(x, y + this.OffsetY))
            {
                action.Invoke();
                return;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        foreach (var (_, component, _, _, action) in this.items)
        {
            if (component.containsPoint(x, y + this.OffsetY))
            {
                action?.Invoke();
                return;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        const int scrollAmount = 40;

        // Scroll down
        if (direction < 0)
        {
            this.OffsetY += scrollAmount;
            Game1.playSound("shiny4");
            return;
        }

        // Scroll up
        if (direction > 0)
        {
            this.OffsetY -= scrollAmount;
            Game1.playSound("shiny4");
        }
    }

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
        this.MaxOffset = Math.Max(0, currentY - this.yPositionOnScreen - this.height);
        this.offsetY = Math.Min(Math.Max(0, this.offsetY), this.MaxOffset);
        this.populateClickableComponentList();
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

            var text = expression.ExpressionType switch
            {
                ExpressionType.All => I18n.Ui_All_Name(),
                ExpressionType.Any => I18n.Ui_Any_Name(),
                ExpressionType.Not => I18n.Ui_Not_Name(),
            };

            var tooltip = expression.ExpressionType switch
            {
                ExpressionType.All => I18n.Ui_All_Tooltip(),
                ExpressionType.Any => I18n.Ui_Any_Tooltip(),
                ExpressionType.Not => I18n.Ui_Not_Tooltip(),
            };

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
                {
                    foreach (var subExpression in expression.Expressions)
                    {
                        Enqueue(subExpression);
                    }

                    AddInsert(expression);
                    break;
                }

                case ExpressionType.Not:
                {
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
                    () => this.Add(expression, ExpressionType.Static)));

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

                //new Rectangle(337, 494, 12, 12),
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
                I18n.Ui_Remove_Tooltip(),
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
        this.parent.OverrideSearchText(this.baseExpression.Text);
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

    private void ChangeAttribute(IExpression toChange, ItemAttribute attribute)
    {
        var dynamicTerm = toChange.Expressions.ElementAtOrDefault(0);
        if (this.baseExpression is null || dynamicTerm?.ExpressionType is not ExpressionType.Dynamic)
        {
            return;
        }

        dynamicTerm.Term = attribute.ToStringFast();
        this.parent.OverrideSearchText(this.baseExpression.Text);
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
        this.parent.OverrideSearchText(this.baseExpression.Text);
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

    private void ShowDropdown(IExpression expression, ClickableComponent component) =>
        this.parent.DropDown = new Dropdown<ItemAttribute>(
            ItemAttributeExtensions.GetValues().Select(i => (i, i.ToStringFast())).ToList(),
            component.bounds.X,
            component.bounds.Bottom - this.OffsetY,
            value =>
            {
                if (value is not null)
                {
                    this.ChangeAttribute(expression, (ItemAttribute)value);
                }

                this.parent.DropDown = null;
            });

    private void ShowPopup(IExpression expression, ClickableComponent component) =>
        this.parent.SetChildMenu(
            new BasePopupList(component.label ?? string.Empty, ItemAttributeExtensions.GetNames().ToList()));

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
        this.parent.OverrideSearchText(this.baseExpression.Text);
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