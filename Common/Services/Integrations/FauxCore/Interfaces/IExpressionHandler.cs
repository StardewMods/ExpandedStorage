namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling parsed expressions.</summary>
public interface IExpressionHandler
{
    /// <summary>Attempts to parse the given expression.</summary>
    /// <param name="expression">The expression to be parsed.</param>
    /// <param name="parsedExpression">The parsed expression.</param>
    /// <returns><c>true</c> if the expression could be parsed; otherwise, <c>false</c>.</returns>
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out IExpression? parsedExpression);
}