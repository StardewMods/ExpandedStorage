namespace StardewMods.BetterChests.Framework.Models.Terms;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a negated term.</summary>
internal sealed class NotExpression : ISearchExpression
{
    /// <summary>The negation character.</summary>
    public const char Char = '!';

    /// <summary>The expression parser.</summary>
    public static readonly Parser<char, ISearchExpression> ExpressionParser = Parser
        .Char(NotExpression.Char)
        .Then(Parser.OneOf(AnyExpression.ExpressionParser, AllExpression.ExpressionParser, SearchTerm.TermParser))
        .Between(Parser.SkipWhitespaces)
        .Select(term => new NotExpression(term))
        .OfType<ISearchExpression>();

    /// <summary>Initializes a new instance of the <see cref="NotExpression" /> class.</summary>
    /// <param name="expression">The negated term.</param>
    private NotExpression(ISearchExpression expression) => this.InnerExpression = expression;

    /// <summary>Gets the negated term.</summary>
    public ISearchExpression InnerExpression { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item item) => !this.InnerExpression.ExactMatch(item);

    /// <inheritdoc />
    public bool PartialMatch(Item item) => !this.InnerExpression.PartialMatch(item);

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) => !this.InnerExpression.ExactMatch(container);

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) => !this.InnerExpression.ExactMatch(container);

    /// <inheritdoc />
    public override string ToString() => $"({NotExpression.Char} {this.InnerExpression})";
}