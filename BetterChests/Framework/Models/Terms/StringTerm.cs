namespace StardewMods.BetterChests.Framework.Models.Terms;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a basic term.</summary>
internal sealed class StringTerm : ISearchExpression
{
    /// <summary>The search term parser.</summary>
#if DEBUG
    public static Parser<char, ISearchExpression> TermParser =>
        Parser
            .AnyCharExcept(
                AnyExpression.BeginChar,
                AnyExpression.EndChar,
                AllExpression.BeginChar,
                AllExpression.EndChar,
                NotExpression.Char,
                MatchExpression.Char,
                ' ')
            .ManyString()
            .Between(Parser.SkipWhitespaces)
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Select(term => new StringTerm(term))
            .OfType<ISearchExpression>();
#else
    public static readonly Parser<char, ISearchExpression> TermParser = Parser
        .AnyCharExcept(
            AnyExpression.BeginChar,
            AnyExpression.EndChar,
            AllExpression.BeginChar,
            AllExpression.EndChar,
            NotExpression.Char,
            MatchExpression.Char,
            ' ')
        .ManyString()
        .Between(Parser.SkipWhitespaces)
        .Where(term => !string.IsNullOrWhiteSpace(term))
        .Select(term => new StringTerm(term))
        .OfType<ISearchExpression>();
#endif

    /// <summary>Initializes a new instance of the <see cref="StringTerm" /> class.</summary>
    /// <param name="term">The search value.</param>
    private StringTerm(string term) => this.Term = term;

    /// <summary>Gets the value.</summary>
    public string Term { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item? item) =>
        item is not null
        && ((item.Name is not null && this.ExactMatch(item.Name))
            || (item.DisplayName is not null && this.ExactMatch(item.DisplayName))
            || item.GetContextTags().Any(this.ExactMatch));

    /// <inheritdoc />
    public bool PartialMatch(Item? item) =>
        item is not null
        && ((item.Name is not null && this.PartialMatch(item.Name))
            || (item.DisplayName is not null && this.PartialMatch(item.DisplayName))
            || item.GetContextTags().Any(this.PartialMatch));

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) =>
        this.ExactMatch(container.ToString()!) || container.Items.Any(this.ExactMatch);

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) =>
        this.PartialMatch(container.ToString()!) || container.Items.Any(this.PartialMatch);

    /// <inheritdoc />
    public override string ToString() => this.Term;

    private bool ExactMatch(string term) =>
        !string.IsNullOrWhiteSpace(term) && term.Equals(this.Term, StringComparison.OrdinalIgnoreCase);

    private bool PartialMatch(string term) =>
        !string.IsNullOrWhiteSpace(term) && term.Contains(this.Term, StringComparison.OrdinalIgnoreCase);
}