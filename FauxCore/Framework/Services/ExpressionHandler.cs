namespace StardewMods.FauxCore.Framework.Services;

using System.Text;
using Force.DeepCloner;
using Pidgin;
using StardewMods.Common.Models.Cache;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Models.Expressions;

/// <summary>Responsible for handling parsed expressions.</summary>
internal sealed class ExpressionHandler : GenericBaseService<ExpressionHandler>, IExpressionHandler
{
    private static readonly Parser<char, IExpression> AllParser;
    private static readonly Parser<char, IExpression> AnyParser;
    private static readonly Parser<char, IExpression> ComparableParser;
    private static readonly Parser<char, IExpression> DynamicParser;
    private static readonly Parser<char, IExpression> NotParser;
    private static readonly Parser<char, IExpression> QuotedParser;
    private static readonly Parser<char, IExpression> RootParser;
    private static readonly Parser<char, IExpression> StaticParser;

    private readonly CacheTable<IExpression?> cachedSearches;

    static ExpressionHandler()
    {
        var stringParser = Parser
            .AnyCharExcept(
                AnyExpression.BeginChar,
                AnyExpression.EndChar,
                AllExpression.BeginChar,
                AllExpression.EndChar,
                DynamicTerm.BeginChar,
                DynamicTerm.EndChar,
                QuotedTerm.BeginChar,
                QuotedTerm.EndChar,
                NotExpression.Char,
                ComparableExpression.Char,
                ' ')
            .ManyString()
            .Between(Parser.SkipWhitespaces)
            .Where(term => !string.IsNullOrWhiteSpace(term));

        ExpressionHandler.StaticParser = stringParser.Select(term => new StaticTerm(term)).OfType<IExpression>();

        ExpressionHandler.QuotedParser = stringParser
            .Between(Parser.Char(QuotedTerm.BeginChar), Parser.Char(QuotedTerm.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new QuotedTerm(expressions))
            .OfType<IExpression>();

        ExpressionHandler.DynamicParser = stringParser
            .Between(Parser.Char(DynamicTerm.BeginChar), Parser.Char(DynamicTerm.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new DynamicTerm(expressions))
            .OfType<IExpression>();

        ExpressionHandler.ComparableParser = Parser.Try(
            Parser
                .Map(
                    (left, _, right) => new ComparableExpression(left, right),
                    ExpressionHandler.DynamicParser,
                    Parser.Char(ComparableExpression.Char),
                    ExpressionHandler.QuotedParser.Or(ExpressionHandler.StaticParser))
                .OfType<IExpression>());

        var parsers = Parser.Rec(
            () => Parser.OneOf(
                ExpressionHandler.AnyParser!,
                ExpressionHandler.AllParser!,
                ExpressionHandler.NotParser!,
                ExpressionHandler.ComparableParser,
                ExpressionHandler.DynamicParser,
                ExpressionHandler.QuotedParser,
                ExpressionHandler.StaticParser));

        ExpressionHandler.AllParser = parsers
            .Many()
            .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new AllExpression(expressions.ToArray()))
            .OfType<IExpression>();

        ExpressionHandler.AnyParser = parsers
            .Many()
            .Between(Parser.Char(AnyExpression.BeginChar), Parser.Char(AnyExpression.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new AnyExpression(expressions.ToArray()))
            .OfType<IExpression>();

        ExpressionHandler.NotParser = Parser
            .Char(NotExpression.Char)
            .Then(
                Parser.OneOf(
                    ExpressionHandler.AnyParser,
                    ExpressionHandler.AllParser,
                    ExpressionHandler.ComparableParser,
                    ExpressionHandler.DynamicParser,
                    ExpressionHandler.QuotedParser,
                    ExpressionHandler.StaticParser))
            .Select(term => new NotExpression(term))
            .OfType<IExpression>();

        ExpressionHandler.RootParser = parsers
            .Many()
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new RootExpression(expressions.ToArray()))
            .OfType<IExpression>();
    }

    /// <summary>Initializes a new instance of the <see cref="ExpressionHandler" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ExpressionHandler(CacheManager cacheManager, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.cachedSearches = cacheManager.GetCacheTable<IExpression?>();

    /// <inheritdoc cref="IExpressionHandler" />
    public bool TryCreateExpression(
        ExpressionType expressionType,
        [NotNullWhen(true)] out IExpression? expression,
        string? term = null,
        params IExpression[]? expressions)
    {
        expressions ??= Array.Empty<IExpression>();
        term ??= string.Empty;
        expression = expressionType switch
        {
            ExpressionType.All => new AllExpression(expressions),
            ExpressionType.Any => new AnyExpression(expressions),
            ExpressionType.Comparable => new ComparableExpression(expressions),
            ExpressionType.Not => new NotExpression(expressions),
            ExpressionType.Dynamic => new DynamicTerm(term),
            ExpressionType.Quoted => new QuotedTerm(term),
            ExpressionType.Static => new StaticTerm(term),
            _ => null,
        };

        return expression is not null;
    }

    /// <inheritdoc cref="IExpressionHandler" />
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out IExpression? parsedExpression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            parsedExpression = null;
            return false;
        }

        if (this.cachedSearches.TryGetValue(expression, out var cachedExpression))
        {
            parsedExpression = cachedExpression.DeepClone();
            return parsedExpression is not null;
        }

        // Determine if self-repair may be needed
        var closeChars = new Stack<char>();
        foreach (var c in expression)
        {
            switch (c)
            {
                case '(':
                    closeChars.Push(')');
                    break;
                case ')' when closeChars.Peek() == ')':
                    closeChars.Pop();
                    break;
                case '[':
                    closeChars.Push(']');
                    break;
                case ']' when closeChars.Peek() == ']':
                    closeChars.Pop();
                    break;
                case '{':
                    closeChars.Push('}');
                    break;
                case '}' when closeChars.Peek() == '}':
                    closeChars.Pop();
                    break;
            }
        }

        // Attempt self-repair
        if (closeChars.Any())
        {
            var sb = new StringBuilder(expression);
            while (closeChars.TryPop(out var closeChar))
            {
                sb.Append(closeChar);
            }

            expression = sb.ToString();
        }

        try
        {
            parsedExpression = ExpressionHandler.RootParser.ParseOrThrow(expression);
        }
        catch (ParseException ex)
        {
            this.Log.Trace("Failed to parse search expression {0}.\n{1}", expression, ex);
            parsedExpression = null;
        }

        this.cachedSearches.AddOrUpdate(expression, parsedExpression.DeepClone());
        return parsedExpression is not null;
    }
}