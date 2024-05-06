namespace StardewMods.BetterChests.Framework.Models.Terms;

using System.Collections.Immutable;
using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents an and expression.</summary>
internal sealed class AllExpression : ISearchExpression
{
    /// <summary>The begin group character.</summary>
    public const char BeginChar = '(';

    /// <summary>The end group character.</summary>
    public const char EndChar = ')';

    /// <summary>The expression parser.</summary>
    public static readonly Parser<char, ISearchExpression> ExpressionParser = Parser
        .Rec(
            () => Parser.OneOf(
                AnyExpression.ExpressionParser,
                AllExpression.ExpressionParser!,
                NotExpression.ExpressionParser,
                SearchTerm.TermParser))
        .Many()
        .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
        .Select(expressions => new AllExpression(expressions))
        .OfType<ISearchExpression>();

    /// <summary>Initializes a new instance of the <see cref="AllExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    private AllExpression(IEnumerable<ISearchExpression> expressions) =>
        this.Expressions = expressions.ToImmutableArray();

    /// <summary>Gets the grouped expressions.</summary>
    public ImmutableArray<ISearchExpression> Expressions { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item? item) =>
        item is not null && this.Expressions.All(expression => expression.ExactMatch(item));

    /// <inheritdoc />
    public bool PartialMatch(Item? item) =>
        item is not null && this.Expressions.All(expression => expression.PartialMatch(item));

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) =>
        this.Expressions.All(expression => expression.ExactMatch(container));

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) =>
        this.Expressions.All(expression => expression.PartialMatch(container));

    /// <inheritdoc />
    public override string ToString() =>
        $"{AllExpression.BeginChar}{string.Join(' ', this.Expressions)}{AllExpression.EndChar}";
}