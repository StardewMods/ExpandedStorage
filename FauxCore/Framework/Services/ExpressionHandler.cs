namespace StardewMods.FauxCore.Framework.Services;

using System.Text;
using Force.DeepCloner;
using Pidgin;
using StardewMods.FauxCore.Common.Enums;
using StardewMods.FauxCore.Common.Models.Cache;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Models.Expressions;

/// <summary>Responsible for handling parsed expressions.</summary>
internal sealed class ExpressionHandler : BaseService<ExpressionHandler>, IExpressionHandler
{
    private static readonly IExpression DefaultExpression = new RootExpression();
    private static readonly Parser<char, IExpression> RootParser;

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
            .Where(term => !string.IsNullOrWhiteSpace(term));

        var staticTerm = stringParser
            .Between(Parser.SkipWhitespaces)
            .Select(expression => new StaticTerm(expression))
            .OfType<IExpression>();

        var quotedTerm = stringParser
            .Separated(Parser.Whitespaces)
            .Between(Parser.Char(QuotedTerm.BeginChar), Parser.Char(QuotedTerm.EndChar))
            .Select(expressions => new QuotedTerm(string.Join(' ', expressions)))
            .OfType<IExpression>();

        var dynamicParser = stringParser
            .Separated(Parser.Whitespaces)
            .Between(Parser.Char(DynamicTerm.BeginChar), Parser.Char(DynamicTerm.EndChar))
            .Select(expressions => new DynamicTerm(string.Join(' ', expressions)))
            .OfType<IExpression>();

        var comparableParser = Parser.Try(
            Parser
                .Map(
                    (left, _, right) => new ComparableExpression(left, right),
                    dynamicParser,
                    Parser.Char(ComparableExpression.Char),
                    Parser.OneOf(quotedTerm, staticTerm))
                .OfType<IExpression>());

        var quotedParser = quotedTerm
            .Select(expression => new ComparableExpression(new DynamicTerm(ItemAttribute.Any.ToStringFast()), expression))
            .OfType<IExpression>();

        var staticParser = staticTerm
            .Select(expression => new ComparableExpression(new DynamicTerm(ItemAttribute.Any.ToStringFast()), expression))
            .OfType<IExpression>();

        Parser<char, IExpression> allParser = null!;
        Parser<char, IExpression> anyParser = null!;
        Parser<char, IExpression> notParser = null!;

        var parsers = Parser.OneOf(
                Parser.Rec(() => anyParser),
                Parser.Rec(() => allParser),
                Parser.Rec(() => notParser),
                comparableParser,
                dynamicParser,
                quotedParser,
                staticParser);

        allParser = parsers
            .Many()
            .Between(Parser.Char(AllExpression.BeginChar), Parser.Char(AllExpression.EndChar))
            .Select(expressions => new AllExpression(expressions.ToArray()))
            .OfType<IExpression>();

        anyParser = parsers
            .Many()
            .Between(Parser.Char(AnyExpression.BeginChar), Parser.Char(AnyExpression.EndChar))
            .Select(expressions => new AnyExpression(expressions.ToArray()))
            .OfType<IExpression>();

        notParser = Parser
            .Char(NotExpression.Char)
            .Then(parsers)
            .Select(term => new NotExpression(term))
            .OfType<IExpression>();

        ExpressionHandler.RootParser = parsers
            .Many()
            .Select(expressions => new RootExpression(expressions.ToArray()))
            .OfType<IExpression>();
    }

    /// <summary>Initializes a new instance of the <see cref="ExpressionHandler" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    public ExpressionHandler(CacheManager cacheManager) =>
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
            parsedExpression = ExpressionHandler.DefaultExpression.DeepClone();
            return true;
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
            Log.TraceOnce("Failed to parse search expression {0}.\n{1}", expression, ex);
            parsedExpression = ExpressionHandler.DefaultExpression;
        }

        //this.cachedSearches.AddOrUpdate(expression, parsedExpression.DeepClone());
        return true;
    }
}