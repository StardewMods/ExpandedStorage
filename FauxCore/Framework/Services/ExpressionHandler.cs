namespace StardewMods.FauxCore.Framework.Services;

using System.Text;
using Pidgin;
using StardewMods.Common.Models.Cache;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Models.Expressions;

/// <summary>Responsible for handling parsed expressions.</summary>
internal sealed class ExpressionHandler : GenericBaseService<ExpressionHandler>, IExpressionHandler
{
    private static readonly Parser<char, IExpression> AllExact;
    private static readonly Parser<char, IExpression> AllPartial;
    private static readonly Parser<char, IExpression> AnyExact;
    private static readonly Parser<char, IExpression> AnyPartial;
    private static readonly Parser<char, IExpression> ComparableExact;
    private static readonly Parser<char, IExpression> ComparablePartial;
    private static readonly Parser<char, IExpression> DynamicExact;
    private static readonly Parser<char, IExpression> DynamicPartial;
    private static readonly Parser<char, IExpression> NotExact;
    private static readonly Parser<char, IExpression> NotPartial;
    private static readonly Parser<char, IExpression> StaticExact;
    private static readonly Parser<char, IExpression> StaticPartial;

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
                NotExpression.Char,
                ComparableExpression.Char,
                ' ')
            .ManyString()
            .Between(Parser.SkipWhitespaces)
            .Where(term => !string.IsNullOrWhiteSpace(term));

        ExpressionHandler.StaticExact = stringParser.Select(term => new StaticTerm(term, true)).OfType<IExpression>();
        ExpressionHandler.StaticPartial =
            stringParser.Select(term => new StaticTerm(term, false)).OfType<IExpression>();

        ExpressionHandler.DynamicExact = stringParser
            .Between(Parser.Char(DynamicTerm.BeginChar), Parser.Char(DynamicTerm.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new DynamicTerm(expressions))
            .OfType<IExpression>();

        ExpressionHandler.DynamicPartial = stringParser
            .Between(Parser.Char(DynamicTerm.BeginChar), Parser.Char(DynamicTerm.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new DynamicTerm(expressions))
            .OfType<IExpression>();

        ExpressionHandler.ComparableExact = Parser.Try(
            Parser
                .Map(
                    (left, _, right) => new ComparableExpression((DynamicTerm)left, (StaticTerm)right, true),
                    ExpressionHandler.DynamicExact,
                    Parser.Char(ComparableExpression.Char),
                    ExpressionHandler.StaticExact)
                .OfType<IExpression>());

        ExpressionHandler.ComparablePartial = Parser.Try(
            Parser
                .Map(
                    (left, _, right) => new ComparableExpression((DynamicTerm)left, (StaticTerm)right, false),
                    ExpressionHandler.DynamicPartial,
                    Parser.Char(ComparableExpression.Char),
                    ExpressionHandler.StaticPartial)
                .OfType<IExpression>());

        var exactParser = Parser.Rec(
            () => Parser.OneOf(
                ExpressionHandler.AnyExact!,
                ExpressionHandler.AllExact!,
                ExpressionHandler.NotExact!,
                ExpressionHandler.ComparableExact,
                ExpressionHandler.DynamicExact,
                ExpressionHandler.StaticExact));

        var partialParser = Parser.Rec(
            () => Parser.OneOf(
                ExpressionHandler.AnyPartial!,
                ExpressionHandler.AllPartial!,
                ExpressionHandler.NotPartial!,
                ExpressionHandler.ComparablePartial,
                ExpressionHandler.DynamicPartial,
                ExpressionHandler.StaticPartial));

        ExpressionHandler.AllExact = exactParser
            .Many()
            .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new AllExpression(expressions))
            .OfType<IExpression>();

        ExpressionHandler.AllPartial = partialParser
            .Many()
            .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new AllExpression(expressions))
            .OfType<IExpression>();

        ExpressionHandler.AnyExact = exactParser
            .Many()
            .Between(Parser.Char(AnyExpression.BeginChar), Parser.Char(AnyExpression.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new AnyExpression(expressions))
            .OfType<IExpression>();

        ExpressionHandler.AnyPartial = partialParser
            .Many()
            .Between(Parser.Char(AnyExpression.BeginChar), Parser.Char(AnyExpression.EndChar))
            .Between(Parser.SkipWhitespaces)
            .Select(expressions => new AnyExpression(expressions))
            .OfType<IExpression>();

        ExpressionHandler.NotExact = Parser
            .Char(NotExpression.Char)
            .Then(
                Parser.OneOf(
                    ExpressionHandler.AnyExact,
                    ExpressionHandler.AllExact,
                    ExpressionHandler.ComparableExact,
                    ExpressionHandler.DynamicExact,
                    ExpressionHandler.StaticExact))
            .Select(term => new NotExpression(term))
            .OfType<IExpression>();

        ExpressionHandler.NotPartial = Parser
            .Char(NotExpression.Char)
            .Then(
                Parser.OneOf(
                    ExpressionHandler.AnyPartial,
                    ExpressionHandler.AllPartial,
                    ExpressionHandler.ComparablePartial,
                    ExpressionHandler.DynamicPartial,
                    ExpressionHandler.StaticPartial))
            .Select(term => new NotExpression(term))
            .OfType<IExpression>();
    }

    /// <summary>Initializes a new instance of the <see cref="ExpressionHandler" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ExpressionHandler(CacheManager cacheManager, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.cachedSearches = cacheManager.GetCacheTable<IExpression?>();

    /// <summary>Attempts to parse the given expression.</summary>
    /// <param name="expression">The expression to be parsed.</param>
    /// <param name="parsedExpression">The parsed expression.</param>
    /// <returns><c>true</c> if the expression could be parsed; otherwise, <c>false</c>.</returns>
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out IExpression? parsedExpression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            parsedExpression = null;
            return false;
        }

        if (this.cachedSearches.TryGetValue(expression, out parsedExpression))
        {
            return parsedExpression is not null;
        }

        // Determine if self-repair may be needed
        var openParentheses = 0;
        var openBrackets = 0;
        foreach (var c in expression)
        {
            switch (c)
            {
                case '(':
                    openParentheses++;
                    break;
                case ')':
                    openParentheses--;
                    break;
                case '[':
                    openBrackets++;
                    break;
                case ']':
                    openBrackets--;
                    break;
            }
        }

        // Attempt self-repair
        if (openParentheses > 0 || openBrackets > 0)
        {
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
                }
            }

            var sb = new StringBuilder(expression);
            while (closeChars.TryPop(out var closeChar))
            {
                sb.Append(closeChar);
            }

            expression = sb.ToString();
        }

        try
        {
            parsedExpression = ExpressionHandler.AnyPartial.ParseOrThrow($"[{expression}]");
        }
        catch (ParseException ex)
        {
            this.Log.Trace("Failed to parse search expression {0}.\n{1}", expression, ex);
            parsedExpression = null;
        }

        this.cachedSearches.AddOrUpdate(expression, parsedExpression);
        return parsedExpression is not null;
    }
}