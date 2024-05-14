namespace StardewMods.FauxCore.Framework.Models.Expressions;

using System.Collections.Immutable;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents an individual expression where the left and rights terms must match.</summary>
internal sealed class ComparableExpression : IExpression
{
    /// <summary>The match character.</summary>
    public const char Char = '~';

    private ImmutableList<IExpression> expressions;

    /// <summary>Initializes a new instance of the <see cref="ComparableExpression" /> class.</summary>
    /// <param name="expressions">The left and right expressions.</param>
    public ComparableExpression(params IExpression[] expressions)
    {
        var terms = new IExpression[2];
        terms[0] = expressions.ElementAtOrDefault(0) as DynamicTerm
            ?? new DynamicTerm(ItemAttribute.Any.ToStringFast());

        terms[1] = expressions.ElementAtOrDefault(1) as StaticTerm ?? new StaticTerm(string.Empty);

        this.Expressions = terms.ToImmutableList();
    }

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.Comparable;

    /// <inheritdoc />
    public bool IsValid => this.LeftTerm is not null && !string.IsNullOrWhiteSpace(this.RightTerm?.Term);

    /// <inheritdoc />
    public string Text =>
        this.LeftTerm?.Text.Contains(ItemAttribute.Any.ToStringFast(), StringComparison.OrdinalIgnoreCase) == true
            ? $"{this.RightTerm?.Text}"
            : $"{this.LeftTerm?.Text}{ComparableExpression.Char}{this.RightTerm?.Text}";

    /// <inheritdoc />
    public IImmutableList<IExpression> Expressions
    {
        get => this.expressions;
        [MemberNotNull(nameof(ComparableExpression.expressions))]
        set
        {
            this.expressions = value.ToImmutableList();
            foreach (var expression in this.expressions)
            {
                expression.Parent = this;
            }
        }
    }

    /// <inheritdoc />
    public IExpression? Parent { get; set; }

    /// <inheritdoc />
    public string Term
    {
        get => string.Empty;
        set => throw new NotSupportedException();
    }

    private int? ComparableInt =>
        this.LeftTerm?.TryParse(this.RightTerm?.Term, out var parsedInt) == true ? parsedInt : null;

    private DynamicTerm? LeftTerm => this.Expressions.ElementAtOrDefault(0) as DynamicTerm;

    private StaticTerm? RightTerm => this.Expressions.ElementAtOrDefault(1) as StaticTerm;

    /// <inheritdoc />
    public int Compare(Item? x, Item? y)
    {
        if ((x is null && y is null) || this.LeftTerm is null)
        {
            return 0;
        }

        if (x is null || !this.LeftTerm.TryGetValue(x, out var xValue))
        {
            return -1;
        }

        if (y is null || !this.LeftTerm.TryGetValue(y, out var yValue))
        {
            return 1;
        }

        if (xValue is string xString && yValue is string yString)
        {
            return (this.Equals(xString) ? -1 : 1).CompareTo(this.Equals(yString) ? -1 : 1);
        }

        if (xValue is int xInt && yValue is int yInt)
        {
            if (!this.ComparableInt.HasValue)
            {
                return 0;
            }

            return (this.ComparableInt.Value == xInt ? -1 : 1).CompareTo(this.ComparableInt.Value == yInt ? -1 : 1);
        }

        if (xValue is IEnumerable<string> xList && yValue is IEnumerable<string> yList)
        {
            var xItem = xList.FirstOrDefault(this.Equals);
            var yItem = yList.FirstOrDefault(this.Equals);
            if (xItem is null && yItem is null)
            {
                return 0;
            }

            if (xItem is null)
            {
                return -1;
            }

            if (yItem is null)
            {
                return 1;
            }

            return string.Compare(xItem, yItem, StringComparison.OrdinalIgnoreCase);
        }

        return 0;
    }

    /// <inheritdoc />
    public bool Equals(Item? item) =>
        this.LeftTerm is not null
        && this.LeftTerm.TryGetValue(item, out var itemValue)
        && itemValue switch
        {
            string value => this.Equals(value),
            int value => this.ComparableInt.HasValue && this.ComparableInt.Value == value,
            IEnumerable<string> value => value.Any(this.Equals),
            _ => false,
        };

    /// <inheritdoc />
    public bool Equals(IInventory? other) => other is not null && other.Any(this.Equals);

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other)
        && !string.IsNullOrWhiteSpace(this.RightTerm?.Term)
        && other.Contains(this.RightTerm.Term, StringComparison.OrdinalIgnoreCase);
}