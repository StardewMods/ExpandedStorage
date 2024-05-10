namespace StardewMods.FauxCore.Framework.Models.Expressions;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents an individual expression where the sub-expression must not match.</summary>
internal sealed class NotExpression : IExpression
{
    /// <summary>The negation character.</summary>
    public const char Char = '!';

    private readonly IExpression expression;

    /// <summary>Initializes a new instance of the <see cref="NotExpression" /> class.</summary>
    /// <param name="expression">The negated term.</param>
    public NotExpression(IExpression expression)
    {
        this.expression = expression;
        this.Expressions = [expression];
    }

    /// <inheritdoc />
    public IEnumerable<IExpression> Expressions { get; }

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.Not;

    /// <inheritdoc />
    public string? Term => null;

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => -this.expression.Compare(x, y);

    /// <inheritdoc />
    public bool Equals(Item? other) => !this.expression.Equals(other);

    /// <inheritdoc />
    public bool Equals(IInventory? other) => other is not null && !this.expression.Equals(other);

    /// <inheritdoc />
    public bool Equals(string? other) => !string.IsNullOrWhiteSpace(other) && !this.expression.Equals(other);

    /// <inheritdoc />
    public override string ToString() => $"({NotExpression.Char} {this.expression})";
}