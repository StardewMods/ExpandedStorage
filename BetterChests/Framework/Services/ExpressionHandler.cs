namespace StardewMods.BetterChests.Framework.Services;

using System.Text;
using Pidgin;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Expressions;
using StardewMods.Common.Models.Cache;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling parsed expressions.</summary>
internal sealed class ExpressionHandler : GenericBaseService<ExpressionHandler>
{
    private readonly CacheTable<IExpression?> cachedSearches;
    private readonly PerScreen<IExpression?> searchExpression = new();
    private readonly PerScreen<string> searchText = new(() => string.Empty);

    /// <summary>Initializes a new instance of the <see cref="ExpressionHandler" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ExpressionHandler(CacheManager cacheManager, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.cachedSearches = cacheManager.GetCacheTable<IExpression?>();

    /// <summary>Gets or sets the current search expression.</summary>
    public IExpression? SearchExpression
    {
        get => this.searchExpression.Value;
        set => this.searchExpression.Value = value;
    }

    /// <summary>Gets or sets the current search text.</summary>
    public string SearchText
    {
        get => this.searchText.Value;
        set => this.searchText.Value = value;
    }

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
            parsedExpression = AnyExpression.PartialParser.ParseOrThrow($"[{expression}]");
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