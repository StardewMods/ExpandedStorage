namespace StardewMods.BetterChests.Framework.Models.Expressions;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents an individual expression where the sub-expression must not match.</summary>
internal sealed class NotExpression : IExpression
{
    /// <summary>The negation character.</summary>
    public const char Char = '!';

    /// <summary>An exact expression parser.</summary>
    public static readonly Parser<char, IExpression> ExactParser = Parser
        .Char(NotExpression.Char)
        .Then(
            Parser.OneOf(
                AnyExpression.ExactParser,
                AllExpression.ExactParser,
                ComparableExpression.ExactParser,
                DynamicTerm.ExactParser,
                StaticTerm.ExactParser))
        .Select(term => new NotExpression(term))
        .OfType<IExpression>();

    /// <summary>A partial expression parser.</summary>
    public static readonly Parser<char, IExpression> PartialParser = Parser
        .Char(NotExpression.Char)
        .Then(
            Parser.OneOf(
                AnyExpression.PartialParser,
                AllExpression.PartialParser,
                ComparableExpression.PartialParser,
                DynamicTerm.PartialParser,
                StaticTerm.PartialParser))
        .Select(term => new NotExpression(term))
        .OfType<IExpression>();

    /// <summary>Initializes a new instance of the <see cref="NotExpression" /> class.</summary>
    /// <param name="expression">The negated term.</param>
    private NotExpression(IExpression expression) => this.Expression = expression;

    /// <summary>Gets the negated sub-expression.</summary>
    public IExpression Expression { get; }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => -this.Expression.Compare(x, y);

    /// <inheritdoc />
    public bool Matches(Item item) => !this.Expression.Matches(item);

    /// <inheritdoc />
    public bool Matches(IStorageContainer container) => !this.Expression.Matches(container);

    /// <inheritdoc />
    public override string ToString() => $"({NotExpression.Char} {this.Expression})";
}