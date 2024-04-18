namespace StardewMods.BetterChests.Framework.Models.Terms;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Represents a negated term.</summary>
internal sealed class NegatedExpression : ISearchExpression
{
    /// <summary>Initializes a new instance of the <see cref="NegatedExpression" /> class.</summary>
    /// <param name="expression">The negated term.</param>
    public NegatedExpression(ISearchExpression expression) => this.InnerExpression = expression;

    /// <summary>Gets the negated term.</summary>
    public ISearchExpression InnerExpression { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item item) => !this.InnerExpression.ExactMatch(item);

    /// <inheritdoc />
    public bool PartialMatch(Item item) => !this.InnerExpression.PartialMatch(item);
}