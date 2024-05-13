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

    private ImmutableList<IExpression> expressions;

    /// <summary>Initializes a new instance of the <see cref="AllExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    public AllExpression(params IExpression[] expressions) => this.Expressions = expressions.ToImmutableList();

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.All;

    /// <inheritdoc />
    public bool IsValid => this.Expressions.Any();

    /// <inheritdoc />
    public string Text =>
        $"{AllExpression.BeginChar}{string.Join(' ', this.Expressions.Select(expression => expression.Text))}{AllExpression.EndChar}";

    /// <inheritdoc />
    public IImmutableList<IExpression> Expressions
    {
        get => this.expressions;
        [MemberNotNull(nameof(AllExpression.expressions))]
        set
        {
            if (this.expressions is not null)
            {
                foreach (var expression in this.expressions)
                {
                    expression.Parent = null;
                }
            }

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

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => (this.Equals(x) ? 1 : -1).CompareTo(this.Equals(y) ? 1 : -1);

    /// <inheritdoc />
    public bool Equals(Item? item) =>
        item is not null && (!this.Expressions.Any() || this.Expressions.All(expression => expression.Equals(item)));

    /// <inheritdoc />
    public bool Equals(IInventory? other) =>
        other is not null && (!this.Expressions.Any() || this.Expressions.All(expression => expression.Equals(other)));

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other)
        && (!this.Expressions.Any() || this.Expressions.All(expression => expression.Equals(other)));
}