namespace StardewMods.FauxCore.Framework.Models.Expressions;

using StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class RootExpression : AnyExpression
{
    /// <summary>Initializes a new instance of the <see cref="RootExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    public RootExpression(params IExpression[] expressions)
        : base(expressions) { }

    /// <inheritdoc />
    public override string Text => $"{string.Join(' ', this.Expressions.Select(expression => expression.Text))}";
}