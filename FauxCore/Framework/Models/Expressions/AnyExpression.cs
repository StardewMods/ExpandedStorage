namespace StardewMods.FauxCore.Framework.Models.Expressions;

using System.Collections.Immutable;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents a grouped expression where any sub-expressions must match.</summary>
internal class AnyExpression : IExpression
{
    /// <summary>The begin group character.</summary>
    public const char BeginChar = '[';

    /// <summary>The end group character.</summary>
    public const char EndChar = ']';

    private ImmutableList<IExpression> expressions;

    /// <summary>Initializes a new instance of the <see cref="AnyExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    public AnyExpression(params IExpression[] expressions) => this.Expressions = expressions.ToImmutableList();

    /// <inheritdoc />
    public IImmutableList<IExpression> Expressions
    {
        get => this.expressions;
        [MemberNotNull(nameof(AnyExpression.expressions))]
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
    public ExpressionType ExpressionType => ExpressionType.Any;

    /// <inheritdoc />
    public bool IsValid => this.Expressions.Any();

    /// <inheritdoc />
    public IExpression? Parent { get; set; }

    /// <inheritdoc />
    public string Term
    {
        get => string.Empty;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public virtual string Text =>
        $"{AnyExpression.BeginChar}{string.Join(' ', this.Expressions.Select(expression => expression.Text))}{AnyExpression.EndChar}";

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) =>
        !this.Expressions.Any()
            ? 0
            : this
                .Expressions.Select(expression => expression.Compare(x, y))
                .FirstOrDefault(comparison => comparison != 0);

    /// <inheritdoc />
    public bool Equals(Item? item) =>
        item is not null && (!this.Expressions.Any() || this.Expressions.Any(expression => expression.Equals(item)));

    /// <inheritdoc />
    public bool Equals(IInventory? other) =>
        other is not null && (!this.Expressions.Any() || this.Expressions.Any(expression => expression.Equals(other)));

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other)
        && (!this.Expressions.Any() || this.Expressions.Any(expression => expression.Equals(other)));
}