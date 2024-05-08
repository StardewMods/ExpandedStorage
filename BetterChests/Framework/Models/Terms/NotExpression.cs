namespace StardewMods.BetterChests.Framework.Models.Terms;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents an individual expression where the sub-expression must not match.</summary>
internal sealed class NotExpression : ISearchExpression
{
    /// <summary>The negation character.</summary>
    public const char Char = '!';

    /// <summary>The expression parser.</summary>
#if DEBUG
    public static Parser<char, ISearchExpression> ExpressionParser =>
        Parser
            .Char(NotExpression.Char)
            .Then(
                Parser.OneOf(
                    AnyExpression.ExpressionParser,
                    AllExpression.ExpressionParser,
                    MatchExpression.ExpressionParser,
                    StringTerm.TermParser))
            .Select(term => new NotExpression(term))
            .OfType<ISearchExpression>();
#else
    public static readonly Parser<char, ISearchExpression> ExpressionParser = Parser
        .Char(NotExpression.Char)
        .Then(
            Parser.OneOf(
                AnyExpression.ExpressionParser,
                AllExpression.ExpressionParser,
                MatchExpression.ExpressionParser,
                StringTerm.TermParser))
        .Between(Parser.SkipWhitespaces)
        .Select(term => new NotExpression(term))
        .OfType<ISearchExpression>();
#endif

    /// <summary>Initializes a new instance of the <see cref="NotExpression" /> class.</summary>
    /// <param name="expression">The negated term.</param>
    private NotExpression(ISearchExpression expression) => this.Expression = expression;

    /// <summary>Gets the negated sub-expression.</summary>
    public ISearchExpression Expression { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item item) => !this.Expression.ExactMatch(item);

    /// <inheritdoc />
    public bool PartialMatch(Item item) => !this.Expression.PartialMatch(item);

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) => !this.Expression.ExactMatch(container);

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) => !this.Expression.ExactMatch(container);

    /// <inheritdoc />
    public override string ToString() => $"({NotExpression.Char} {this.Expression})";
}