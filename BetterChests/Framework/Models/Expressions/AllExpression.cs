namespace StardewMods.BetterChests.Framework.Models.Expressions;

using System.Collections.Immutable;
using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a grouped expression where all sub-expressions must match.</summary>
internal sealed class AllExpression : IExpression
{
    /// <summary>The begin group character.</summary>
    public const char BeginChar = '(';

    /// <summary>The end group character.</summary>
    public const char EndChar = ')';

    /// <summary>An exact expression parser.</summary>
    public static readonly Parser<char, IExpression> ExactParser = Parser
        .Rec(
            () => Parser.OneOf(
                AnyExpression.ExactParser,
                AllExpression.ExactParser!,
                NotExpression.ExactParser,
                ComparableExpression.ExactParser,
                DynamicTerm.ExactParser,
                StaticTerm.ExactParser))
        .Many()
        .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
        .Between(Parser.SkipWhitespaces)
        .Select(expressions => new AllExpression(expressions))
        .OfType<IExpression>();

    /// <summary>A partial expression parser.</summary>
    public static readonly Parser<char, IExpression> PartialParser = Parser
        .Rec(
            () => Parser.OneOf(
                AnyExpression.PartialParser,
                AllExpression.PartialParser!,
                NotExpression.PartialParser,
                ComparableExpression.PartialParser,
                DynamicTerm.PartialParser,
                StaticTerm.PartialParser))
        .Many()
        .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
        .Between(Parser.SkipWhitespaces)
        .Select(expressions => new AllExpression(expressions))
        .OfType<IExpression>();

    /// <summary>Initializes a new instance of the <see cref="AllExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    private AllExpression(IEnumerable<IExpression> expressions) => this.Expressions = expressions.ToImmutableArray();

    /// <summary>Gets the grouped sub-expressions.</summary>
    public ImmutableArray<IExpression> Expressions { get; }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => (this.Matches(x) ? 1 : -1).CompareTo(this.Matches(y) ? 1 : -1);

    /// <inheritdoc />
    public bool Matches(Item? item) => item is not null && this.Expressions.All(expression => expression.Matches(item));

    /// <inheritdoc />
    public bool Matches(IStorageContainer container) =>
        this.Expressions.All(expression => expression.Matches(container));

    /// <inheritdoc />
    public override string ToString() =>
        $"{AllExpression.BeginChar}{string.Join(' ', this.Expressions)}{AllExpression.EndChar}";
}