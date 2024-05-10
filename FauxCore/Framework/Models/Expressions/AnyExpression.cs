namespace StardewMods.FauxCore.Framework.Models.Expressions;

using System.Collections.Immutable;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents a grouped expression where any sub-expressions must match.</summary>
internal sealed class AnyExpression : IExpression
{
    /// <summary>The begin group character.</summary>
    public const char BeginChar = '[';

    /// <summary>The end group character.</summary>
    public const char EndChar = ']';

    /// <summary>Initializes a new instance of the <see cref="AnyExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    public AnyExpression(IEnumerable<IExpression> expressions) => this.Expressions = expressions.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<IExpression> Expressions { get; }

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.Any;

    /// <inheritdoc />
    public string? Term => null;

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) =>
        this.Expressions.Select(expression => expression.Compare(x, y)).FirstOrDefault(comparison => comparison != 0);

    /// <inheritdoc />
    public bool Equals(Item? item) => item is not null && this.Expressions.Any(expression => expression.Equals(item));

    /// <inheritdoc />
    public bool Equals(IInventory? other) =>
        other is not null && this.Expressions.Any(expression => expression.Equals(other));

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other) && this.Expressions.Any(expression => expression.Equals(other));

    /// <inheritdoc />
    public override string ToString() =>
        $"{AnyExpression.BeginChar}{string.Join(' ', this.Expressions)}{AnyExpression.EndChar}";
}