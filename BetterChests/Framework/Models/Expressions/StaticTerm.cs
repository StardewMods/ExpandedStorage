namespace StardewMods.BetterChests.Framework.Models.Expressions;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a basic term.</summary>
internal sealed class StaticTerm : IExpression
{
    /// <summary>An exact expression parser.</summary>
    public static readonly Parser<char, IExpression> ExactParser;

    /// <summary>A partial expression parser.</summary>
    public static readonly Parser<char, IExpression> PartialParser;

    /// <summary>The string parser.</summary>
    public static readonly Parser<char, string> StringParser;

    private readonly bool exact;

    static StaticTerm()
    {
        StaticTerm.StringParser = Parser
            .AnyCharExcept(
                AnyExpression.BeginChar,
                AnyExpression.EndChar,
                AllExpression.BeginChar,
                AllExpression.EndChar,
                DynamicTerm.BeginChar,
                DynamicTerm.EndChar,
                NotExpression.Char,
                ComparableExpression.Char,
                ' ')
            .ManyString()
            .Between(Parser.SkipWhitespaces)
            .Where(term => !string.IsNullOrWhiteSpace(term));

        StaticTerm.ExactParser =
            StaticTerm.StringParser.Select(term => new StaticTerm(term, true)).OfType<IExpression>();

        StaticTerm.PartialParser =
            StaticTerm.StringParser.Select(term => new StaticTerm(term, false)).OfType<IExpression>();
    }

    /// <summary>Initializes a new instance of the <see cref="StaticTerm" /> class.</summary>
    /// <param name="term">The search value.</param>
    /// <param name="exact">Indicates whether exact matching should be used.</param>
    private StaticTerm(string term, bool exact)
    {
        this.exact = exact;
        this.Term = term;
    }

    /// <summary>Gets the value.</summary>
    public string Term { get; }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => (this.Matches(x) ? -1 : 1).CompareTo(this.Matches(y) ? -1 : 1);

    /// <inheritdoc />
    public bool Matches(Item? item) =>
        item is not null
        && ((item.DisplayName is not null && this.Equals(item.DisplayName)) || item.GetContextTags().Any(this.Equals));

    /// <inheritdoc />
    public bool Matches(IStorageContainer container) =>
        this.Equals(container.ToString()!) || container.Items.Any(this.Matches);

    /// <inheritdoc />
    public override string ToString() => this.Term;

    private bool Equals(string term) =>
        this.exact
            ? string.Equals(term, this.Term, StringComparison.OrdinalIgnoreCase)
            : !string.IsNullOrWhiteSpace(term) && term.Contains(this.Term, StringComparison.OrdinalIgnoreCase);
}