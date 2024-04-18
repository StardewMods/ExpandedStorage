namespace StardewMods.BetterChests.Framework.Services;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Terms;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling search.</summary>
internal sealed class SearchHandler : BaseService<SearchHandler>
{
    private static readonly Parser<char, char> BeginGroup;
    private static readonly Parser<char, char> EndGroup;
    private static readonly Parser<char, Unit> LogicalAnd;
    private static readonly Parser<char, Unit> LogicalOr;

    private static readonly Parser<char, ISearchExpression> GroupedExpressionParser;
    private static readonly Parser<char, ISearchExpression> NegatedParser;
    private static readonly Parser<char, ISearchExpression> AndParser;
    private static readonly Parser<char, ISearchExpression> OrParser;
    private static readonly Parser<char, ISearchExpression> TermParser;
    private static readonly Parser<char, ISearchExpression> SearchParser;

    static SearchHandler()
    {
        SearchHandler.BeginGroup = Parser.Char('(');
        SearchHandler.EndGroup = Parser.Char(')');
        SearchHandler.LogicalAnd = Parser.CIString("AND").Then(Parser.SkipWhitespaces).IgnoreResult();
        SearchHandler.LogicalOr = Parser.CIString("OR").Then(Parser.SkipWhitespaces).IgnoreResult();

        SearchHandler.TermParser = Parser
            .AnyCharExcept('(', ')', ' ')
            .ManyString()
            .Between(Parser.SkipWhitespaces)
            .Select(term => (ISearchExpression)new SearchTerm(term));

        SearchHandler.SearchParser = null!;
        var searchExpression = Parser.Rec(() => SearchHandler.SearchParser);

        SearchHandler.GroupedExpressionParser = searchExpression
            .Between(SearchHandler.BeginGroup, SearchHandler.EndGroup)
            .Between(Parser.SkipWhitespaces);

        SearchHandler.NegatedParser = Parser
            .Char('!')
            .Then(searchExpression)
            .Select(term => (ISearchExpression)new NegatedExpression(term));

        var nonRecursiveSearchParser = Parser.OneOf(
            SearchHandler.NegatedParser,
            SearchHandler.GroupedExpressionParser,
            SearchHandler.TermParser);

        SearchHandler.AndParser = Parser.Try(
            Parser.Map(
                (left, right) => (ISearchExpression)new AndExpression(left, right),
                nonRecursiveSearchParser,
                SearchHandler.LogicalAnd.Then(nonRecursiveSearchParser)));

        SearchHandler.OrParser = Parser.Try(
            Parser.Map(
                (left, right) => (ISearchExpression)new OrExpression(left, right),
                nonRecursiveSearchParser,
                SearchHandler.LogicalOr.Then(nonRecursiveSearchParser)));

        SearchHandler.SearchParser = Parser.OneOf(
            SearchHandler.AndParser,
            SearchHandler.OrParser,
            SearchHandler.NegatedParser,
            SearchHandler.GroupedExpressionParser,
            SearchHandler.TermParser);
    }

    /// <summary>Initializes a new instance of the <see cref="SearchHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public SearchHandler(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Searches for items that exactly match the specified expression.</summary>
    /// <param name="expression">The search expression to evaluate.</param>
    /// <param name="items">The collection of items to search within.</param>
    /// <returns>A collection of items that match the specified expression.</returns>
    public IEnumerable<Item> ExactSearch(string expression, IEnumerable<Item> items) =>
        this.TryParseExpression(expression, out var parsedExpression)
            ? items.Where(item => parsedExpression.ExactMatch(item))
            : Enumerable.Empty<Item>();

    /// <summary>Searches for items that partially match the specified expression.</summary>
    /// <param name="expression">The search expression to evaluate.</param>
    /// <param name="items">The collection of items to search within.</param>
    /// <returns>A collection of items that match the specified expression.</returns>
    public IEnumerable<Item> PartialSearch(string expression, IEnumerable<Item> items) =>
        this.TryParseExpression(expression, out var parsedExpression)
            ? items.Where(item => parsedExpression.PartialMatch(item))
            : Enumerable.Empty<Item>();

    /// <summary>Attempts to parse the given search expression.</summary>
    /// <param name="expression">The search expression to be parsed.</param>
    /// <param name="searchExpression">The parsed search expression.</param>
    /// <returns>true if the search expression could be parsed; otherwise, false.</returns>
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out ISearchExpression? searchExpression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            searchExpression = null;
            return false;
        }

        try
        {
            searchExpression = SearchHandler.SearchParser.ParseOrThrow(expression);
            return true;
        }
        catch (ParseException ex)
        {
            this.Log.Trace("Failed to parse search expression {0}.\n{1}", expression, ex);
            searchExpression = null;
            return false;
        }
    }
}