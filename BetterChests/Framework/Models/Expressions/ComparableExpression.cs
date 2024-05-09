namespace StardewMods.BetterChests.Framework.Models.Expressions;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents an individual expression where the left and rights terms must match.</summary>
internal sealed class ComparableExpression : IExpression
{
    /// <summary>The match character.</summary>
    public const char Char = '~';

    /// <summary>An exact expression parser.</summary>
    public static readonly Parser<char, IExpression> ExactParser = Parser.Try(
        Parser
            .Map(
                (left, _, right) => new ComparableExpression((DynamicTerm)left, (StaticTerm)right, true),
                DynamicTerm.ExactParser,
                Parser.Char(ComparableExpression.Char),
                StaticTerm.ExactParser)
            .OfType<IExpression>());

    /// <summary>A partial expression parser.</summary>
    public static readonly Parser<char, IExpression> PartialParser = Parser.Try(
        Parser
            .Map(
                (left, _, right) => new ComparableExpression((DynamicTerm)left, (StaticTerm)right, false),
                DynamicTerm.PartialParser,
                Parser.Char(ComparableExpression.Char),
                StaticTerm.PartialParser)
            .OfType<IExpression>());

    private readonly int? comparableInt;
    private readonly bool exact;

    /// <summary>Initializes a new instance of the <see cref="ComparableExpression" /> class.</summary>
    /// <param name="leftTerm">The attribute to match.</param>
    /// <param name="rightTerm">The matched term.</param>
    /// <param name="exact">Indicates whether exact matching should be used.</param>
    private ComparableExpression(DynamicTerm leftTerm, StaticTerm rightTerm, bool exact)
    {
        this.exact = exact;
        this.LeftTerm = leftTerm;
        this.RightTerm = rightTerm;
        if (leftTerm.TryParse(rightTerm.Term, out var parsedInt))
        {
            this.comparableInt = parsedInt;
        }
    }

    /// <summary>Gets the attribute sub-expression.</summary>
    public DynamicTerm LeftTerm { get; }

    /// <summary>Gets the matched sub-expression.</summary>
    public StaticTerm RightTerm { get; }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y)
    {
        if (x is null && y is null)
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
    public bool Matches(Item? item) =>
        this.LeftTerm.TryGetValue(item, out var itemValue)
        && itemValue switch
        {
            string value => this.Equals(value),
            int value => this.comparableInt.HasValue && this.comparableInt.Value == value,
            IEnumerable<string> value => value.Any(this.Equals),
            _ => false,
        };

    /// <inheritdoc />
    public bool Matches(IStorageContainer container) => container.Items.Any(this.Matches);

    /// <inheritdoc />
    public override string ToString() => $"{this.LeftTerm}{ComparableExpression.Char}{this.RightTerm}";

    private bool Equals(string value) =>
        this.exact
            ? string.Equals(value, this.RightTerm.Term, StringComparison.OrdinalIgnoreCase)
            : value.Contains(this.RightTerm.Term, StringComparison.OrdinalIgnoreCase);
}