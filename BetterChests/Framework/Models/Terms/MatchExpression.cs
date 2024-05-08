namespace StardewMods.BetterChests.Framework.Models.Terms;

using Pidgin;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents an individual expression where the left and rights terms must match.</summary>
internal sealed class MatchExpression : ISearchExpression
{
    /// <summary>The match character.</summary>
    public const char Char = '~';

    /// <summary>The expression parser.</summary>
#if DEBUG
    public static Parser<char, ISearchExpression> ExpressionParser =>
        Parser.Try(
            Parser
                .Map(
                    (left, _, right) => new MatchExpression((StringTerm)left, (StringTerm)right),
                    StringTerm.TermParser,
                    Parser.Char(MatchExpression.Char),
                    StringTerm.TermParser)
                .OfType<ISearchExpression>());
#else
    public static readonly Parser<char, ISearchExpression> ExpressionParser = Parser.Try(
        Parser
            .Map(
                (left, _, right) => new MatchExpression(left, right),
                StringTerm.TermParser,
                Parser.Char(MatchExpression.Char),
                StringTerm.TermParser)
            .OfType<ISearchExpression>());
#endif

    /// <summary>Initializes a new instance of the <see cref="MatchExpression" /> class.</summary>
    /// <param name="attribute">The attribute to match.</param>
    /// <param name="expression">The matched term.</param>
    private MatchExpression(StringTerm attribute, StringTerm expression)
    {
        this.Expression = expression;
        this.Attribute = ItemAttributeExtensions.TryParse(attribute.Term, out var itemAttribute, true)
            ? itemAttribute
            : null;
    }

    /// <summary>Gets the attribute sub-expression.</summary>
    public ItemAttribute? Attribute { get; }

    /// <summary>Gets the matched sub-expression.</summary>
    public StringTerm Expression { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item? item) =>
        item is not null
        && this.Attribute switch
        {
            ItemAttribute.Category =>
                item.getCategoryName().Equals(this.Expression.Term, StringComparison.OrdinalIgnoreCase),
            ItemAttribute.Name => item.Name.Equals(this.Expression.Term, StringComparison.OrdinalIgnoreCase)
                || item.DisplayName.Equals(this.Expression.Term, StringComparison.OrdinalIgnoreCase),
            ItemAttribute.Quality => ((ItemQuality)item.Quality)
                .ToStringFast()
                .Equals(this.Expression.Term, StringComparison.OrdinalIgnoreCase),
            ItemAttribute.Tags => item
                .GetContextTags()
                .Any(tag => tag.Equals(this.Expression.Term, StringComparison.OrdinalIgnoreCase)),
            _ => false,
        };

    /// <inheritdoc />
    public bool PartialMatch(Item? item) =>
        item is not null
        && this.Attribute switch
        {
            ItemAttribute.Category =>
                item.getCategoryName().Contains(this.Expression.Term, StringComparison.OrdinalIgnoreCase),
            ItemAttribute.Name => item.Name.Contains(this.Expression.Term, StringComparison.OrdinalIgnoreCase)
                || item.DisplayName.Equals(this.Expression.Term, StringComparison.OrdinalIgnoreCase),
            ItemAttribute.Quality => ((ItemQuality)item.Quality)
                .ToStringFast()
                .Contains(this.Expression.Term, StringComparison.OrdinalIgnoreCase),
            ItemAttribute.Tags => item
                .GetContextTags()
                .Any(tag => tag.Contains(this.Expression.Term, StringComparison.OrdinalIgnoreCase)),
            _ => false,
        };

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) => container.Items.Any(this.ExactMatch);

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) => container.Items.Any(this.PartialMatch);

    /// <inheritdoc />
    public override string ToString() => $"({this.Attribute} {MatchExpression.Char} {this.Expression})";
}