namespace StardewMods.FauxCore.Framework.Models.Expressions;

using System.Collections.Immutable;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents an individual expression where the sub-expression must not match.</summary>
internal sealed class NotExpression : IExpression
{
    /// <summary>The negation character.</summary>
    public const char Char = '!';

    private ImmutableList<IExpression> expressions;

    /// <summary>Initializes a new instance of the <see cref="NotExpression" /> class.</summary>
    /// <param name="expressions">The sub-expression.</param>
    public NotExpression(params IExpression[] expressions) => this.Expressions = expressions.ToImmutableList();

    /// <inheritdoc />
    public IImmutableList<IExpression> Expressions
    {
        get => this.expressions;
        [MemberNotNull(nameof(NotExpression.expressions))]
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
    public ExpressionType ExpressionType => ExpressionType.Not;

    /// <inheritdoc />
    public bool IsValid => this.Expression is not null;

    /// <inheritdoc />
    public IExpression? Parent { get; set; }

    /// <inheritdoc />
    public string Term
    {
        get => string.Empty;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public string Text => $"{NotExpression.Char}{this.Expression?.Text}";

    private IExpression? Expression => this.Expressions.ElementAtOrDefault(0);

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => -this.Expression?.Compare(x, y) ?? 0;

    /// <inheritdoc />
    public bool Equals(Item? other) => this.Expression?.Equals(other) == false;

    /// <inheritdoc />
    public bool Equals(IInventory? other) => other is not null && this.Expression?.Equals(other) == false;

    /// <inheritdoc />
    public bool Equals(string? other) => !string.IsNullOrWhiteSpace(other) && this.Expression?.Equals(other) == false;
}