namespace StardewMods.BetterChests.Framework.UI.Menus;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>A sub-menu for editing an <see cref="IExpression" />.</summary>
internal sealed class ExpressionsMenu : FramedMenu
{
    private readonly IExpressionHandler expressionHandler;
    private readonly Func<string> getSearchText;
    private readonly IIconRegistry iconRegistry;
    private readonly Action<string> setSearchText;

    private ExpressionGroup? baseComponent;

    /// <summary>Initializes a new instance of the <see cref="ExpressionsMenu" /> class.</summary>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="getSearchText">A function that gets the current search text.</param>
    /// <param name="setSearchText">An action that sets the current search text.</param>
    /// <param name="xPosition">The x-position of the menu.</param>
    /// <param name="yPosition">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    public ExpressionsMenu(
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
        new(this.xPositionOnScreen - 4, this.yPositionOnScreen - 8, this.width + 8, this.height + 20);

    /// <inheritdoc />
    public override int StepSize => 40;

    private string SearchText
    {
        get => this.getSearchText();
        set => this.setSearchText(value);
    }

    /// <inheritdoc />
    public override void DrawUnder(SpriteBatch b, Point cursor)
    { }

    /// <summary>Re-initializes the components of the object with the given initialization expression.</summary>
    /// <param name="initExpression">The initial expression, or null to clear.</param>
    public void ReInitializeComponents(IExpression? initExpression)
    {
        this.allClickableComponents.Clear();
        if (initExpression is null)
        {
            return;
        }

        this.baseComponent = new ExpressionGroup(
            this.iconRegistry,
            this,
            this.Bounds.X,
            this.Bounds.Y,
            this.Bounds.Width,
            initExpression,
            -1);

        this.baseComponent.ExpressionChanged += this.OnExpressionChanged;
        this.allClickableComponents.Add(this.baseComponent);

        this.SetMaxOffset(
            new Point(-1, Math.Max(0, this.baseComponent.bounds.Bottom - this.Bounds.Height - this.Bounds.Y)));
    }

    private void Add(IExpression toAddTo, ExpressionType expressionType)
    {
        if (this.baseComponent is null || !this.expressionHandler.TryCreateExpression(expressionType, out var newChild))
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toAddTo.Expressions = newChildren;
        this.SearchText = this.baseComponent.Expression.Text;
        this.ReInitializeComponents(this.baseComponent.Expression);
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
        if (this.baseComponent?.Expression is null || dynamicTerm?.ExpressionType is not ExpressionType.Dynamic)
        {
            return;
        }

        dynamicTerm.Term = attribute.ToStringFast();
        this.SearchText = this.baseComponent.Expression.Text;
        this.ReInitializeComponents(this.baseComponent.Expression);
    }

    private void ChangeTerm(IExpression toChange, string term)
    {
        var staticTerm = toChange.Expressions.ElementAtOrDefault(1);
        if (this.baseComponent?.Expression is null
            || staticTerm?.ExpressionType is not (ExpressionType.Quoted or ExpressionType.Static))
        {
            return;
        }

        staticTerm.Term = term;
        this.SearchText = this.baseComponent.Expression.Text;
        this.ReInitializeComponents(this.baseComponent.Expression);
    }

    private void OnExpressionChanged(object? sender, ExpressionChangedEventArgs e)
    {
        switch (e.Change)
        {
            case ExpressionChange.AddGroup:
                this.Add(e.Expression, ExpressionType.All);
                break;

            case ExpressionChange.AddNot:
                this.Add(e.Expression, ExpressionType.Not);
                break;

            case ExpressionChange.AddTerm:
                this.Add(e.Expression, ExpressionType.Comparable);
                break;

            case ExpressionChange.ChangeAttribute when sender is ButtonComponent component:
                this.ShowDropdown(e.Expression, component);
                break;

            case ExpressionChange.ChangeValue when sender is ButtonComponent component:
                this.ShowPopup(e.Expression, component);
                break;

            case ExpressionChange.Remove:
                this.Remove(e.Expression);
                break;

            case ExpressionChange.ToggleGroup:
                this.ToggleGroup(e.Expression);
                break;
        }
    }

    private void Remove(IExpression toRemove)
    {
        if (this.baseComponent?.Expression is null || toRemove.Parent is null)
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toRemove.Parent.Expressions = newChildren;
        this.SearchText = this.baseComponent.Expression.Text;
        this.ReInitializeComponents(this.baseComponent.Expression);
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
        popupSelect.OptionSelected += (_, _) => this.ChangeTerm(expression, popupSelect.CurrentText);

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

        if (this.baseComponent?.Expression is null
            || toToggle.Parent is null
            || !this.expressionHandler.TryCreateExpression(expressionType, out var newChild))
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toToggle.Parent.Expressions = newChildren;
        this.SearchText = this.baseComponent.Expression.Text;
        this.ReInitializeComponents(this.baseComponent.Expression);
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