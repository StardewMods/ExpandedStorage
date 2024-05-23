#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
#else
namespace StardewMods.Common.Services.Integrations.FauxCore;
#endif

/// <summary>Responsible for handling parsed expressions.</summary>
public interface IExpressionHandler
{
    /// <summary>Tries to create an expression with the specified parameters.</summary>
    /// <param name="expressionType">The type of the expression to create.</param>
    /// <param name="expression">
    /// When this method returns, contains the created expression, if successful; otherwise,
    /// <c>null</c>.
    /// </param>
    /// <param name="term">A search term used by the expression.</param>
    /// <param name="expressions">A collection of expressions to be used as arguments for the created expression.</param>
    /// <returns><c>true</c> if the expression is successfully created; otherwise, <c>false</c>.</returns>
    public bool TryCreateExpression(
        ExpressionType expressionType,
        [NotNullWhen(true)] out IExpression? expression,
        string? term = null,
        params IExpression[]? expressions);

    /// <summary>Attempts to parse the given expression.</summary>
    /// <param name="expression">The expression to be parsed.</param>
    /// <param name="parsedExpression">The parsed expression.</param>
    /// <returns><c>true</c> if the expression could be parsed; otherwise, <c>false</c>.</returns>
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out IExpression? parsedExpression);
}