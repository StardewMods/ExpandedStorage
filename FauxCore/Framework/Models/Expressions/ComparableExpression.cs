namespace StardewMods.FauxCore.Framework.Models.Expressions;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents an individual expression where the left and rights terms must match.</summary>
internal sealed class ComparableExpression : IExpression
{
    /// <summary>The match character.</summary>
    public const char Char = '~';

    private readonly int? comparableInt;
    private readonly bool exact;
    private readonly DynamicTerm leftTerm;
    private readonly StaticTerm rightTerm;

    /// <summary>Initializes a new instance of the <see cref="ComparableExpression" /> class.</summary>
    /// <param name="leftTerm">The attribute to match.</param>
    /// <param name="rightTerm">The matched term.</param>
    /// <param name="exact">Indicates whether exact matching should be used.</param>
    public ComparableExpression(DynamicTerm leftTerm, StaticTerm rightTerm, bool exact)
    {
        this.exact = exact;
        this.leftTerm = leftTerm;
        this.rightTerm = rightTerm;
        this.Expressions = [leftTerm, rightTerm];
        if (leftTerm.TryParse(rightTerm.Term, out var parsedInt))
        {
            this.comparableInt = parsedInt;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IExpression> Expressions { get; }

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.Comparable;

    /// <inheritdoc />
    public string? Term => null;

    /// <inheritdoc />
    public int Compare(Item? x, Item? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null || !this.leftTerm.TryGetValue(x, out var xValue))
        {
            return -1;
        }

        if (y is null || !this.leftTerm.TryGetValue(y, out var yValue))
        {
            return 1;
        }

        if (xValue is string xString && yValue is string yString)
        {
            return (this.Equals(xString) ? -1 : 1).CompareTo(this.Equals(yString) ? -1 : 1);
        }

        if (xValue is int xInt && yValue is int yInt)
        {
            if (!this.comparableInt.HasValue)
            {
                return 0;
            }

            return (this.comparableInt.Value == xInt ? -1 : 1).CompareTo(this.comparableInt.Value == yInt ? -1 : 1);
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
        this.leftTerm.TryGetValue(item, out var itemValue)
        && itemValue switch
        {
            string value => this.Equals(value),
            int value => this.comparableInt.HasValue && this.comparableInt.Value == value,
            IEnumerable<string> value => value.Any(this.Equals),
            _ => false,
        };

    /// <inheritdoc />
    public bool Equals(IInventory? other) => other is not null && other.Any(this.Equals);

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other)
        && (this.exact
            ? string.Equals(other, this.rightTerm.Term, StringComparison.OrdinalIgnoreCase)
            : other.Contains(this.rightTerm.Term, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => $"{this.leftTerm}{ComparableExpression.Char}{this.rightTerm}";
}