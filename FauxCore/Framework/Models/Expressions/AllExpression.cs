namespace StardewMods.FauxCore.Framework.Models.Expressions;

using System.Collections.Immutable;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents a grouped expression where all sub-expressions must match.</summary>
internal sealed class AllExpression : IExpression
{
    /// <summary>The begin group character.</summary>
    public const char BeginChar = '(';

    /// <summary>The end group character.</summary>
    public const char EndChar = ')';

    /// <summary>Initializes a new instance of the <see cref="AllExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    public AllExpression(IEnumerable<IExpression> expressions) => this.Expressions = expressions.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<IExpression> Expressions { get; }

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.All;

    /// <inheritdoc />
    public string? Term => null;

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => (this.Equals(x) ? 1 : -1).CompareTo(this.Equals(y) ? 1 : -1);

    /// <inheritdoc />
    public bool Equals(Item? item) => item is not null && this.Expressions.All(expression => expression.Equals(item));

    /// <inheritdoc />
    public bool Equals(IInventory? other) =>
        other is not null && this.Expressions.All(expression => expression.Equals(other));

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other) && this.Expressions.All(expression => expression.Equals(other));

    /// <inheritdoc />
    public override string ToString() =>
        $"{AllExpression.BeginChar}{string.Join(' ', this.Expressions)}{AllExpression.EndChar}";
}