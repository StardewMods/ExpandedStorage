namespace StardewMods.BetterChests.Framework.Services;

using System.Text;
using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Terms;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling search.</summary>
internal sealed class SearchHandler : BaseService<SearchHandler>
{
    /// <summary>Initializes a new instance of the <see cref="SearchHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public SearchHandler(ILog log, IManifest manifest)
        : base(log, manifest) { }

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
            searchExpression = AnyExpression.ExpressionParser.ParseOrThrow($"[{expression}]");
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