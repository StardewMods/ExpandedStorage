namespace StardewMods.BetterChests.Framework.Models.Expressions;

using System.Collections.Immutable;
using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a grouped expression where any sub-expressions must match.</summary>
internal sealed class AnyExpression : IExpression
{
    /// <summary>The begin group character.</summary>
    public const char BeginChar = '[';

    /// <summary>The end group character.</summary>
    public const char EndChar = ']';

    /// <summary>An exact expression parser.</summary>
    public static readonly Parser<char, IExpression> ExactParser = Parser
        .Rec(
            () => Parser.OneOf(
                AnyExpression.ExactParser!,
                AllExpression.ExactParser,
                NotExpression.ExactParser,
                ComparableExpression.ExactParser,
                DynamicTerm.ExactParser,
                StaticTerm.ExactParser))
        .Many()
        .Between(Parser.Char(AnyExpression.BeginChar), Parser.Char(AnyExpression.EndChar))
        .Between(Parser.SkipWhitespaces)
        .Select(expressions => new AnyExpression(expressions))
        .OfType<IExpression>();

    /// <summary>A partial expression parser.</summary>
    public static readonly Parser<char, IExpression> PartialParser = Parser
        .Rec(
            () => Parser.OneOf(
                AnyExpression.PartialParser!,
                AllExpression.PartialParser,
                NotExpression.PartialParser,
                ComparableExpression.PartialParser,
                DynamicTerm.PartialParser,
                StaticTerm.PartialParser))
        .Many()
        .Between(Parser.Char(AnyExpression.BeginChar), Parser.Char(AnyExpression.EndChar))
        .Between(Parser.SkipWhitespaces)
        .Select(expressions => new AnyExpression(expressions))
        .OfType<IExpression>();

    /// <summary>Initializes a new instance of the <see cref="AnyExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    private AnyExpression(IEnumerable<IExpression> expressions) => this.Expressions = expressions.ToImmutableArray();

    /// <summary>Gets the grouped sub-expressions.</summary>
    public ImmutableArray<IExpression> Expressions { get; }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) =>
        this.Expressions.Select(expression => expression.Compare(x, y)).FirstOrDefault(comparison => comparison != 0);

    /// <inheritdoc />
    public bool Matches(Item? item) => item is not null && this.Expressions.Any(expression => expression.Matches(item));

    /// <inheritdoc />
    public bool Matches(IStorageContainer container) =>
        this.Expressions.Any(expression => expression.Matches(container));

    /// <inheritdoc />
    public override string ToString() =>
        $"{AnyExpression.BeginChar}{string.Join(' ', this.Expressions)}{AnyExpression.EndChar}";
}