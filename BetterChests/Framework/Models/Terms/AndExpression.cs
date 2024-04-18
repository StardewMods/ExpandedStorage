namespace StardewMods.BetterChests.Framework.Models.Terms;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Represents an and expression.</summary>
internal sealed class AndExpression : ISearchExpression
{
    /// <summary>Initializes a new instance of the <see cref="AndExpression" /> class.</summary>
    /// <param name="leftExpression">The left expression.</param>
    /// <param name="rightExpression">The right expression.</param>
    public AndExpression(ISearchExpression leftExpression, ISearchExpression rightExpression) =>
        (this.LeftExpression, this.RightExpression) = (leftExpression, rightExpression);

    /// <summary>Gets the left expression.</summary>
    public ISearchExpression LeftExpression { get; }

    /// <summary>Gets the right expression.</summary>
    public ISearchExpression RightExpression { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item item) => this.LeftExpression.ExactMatch(item) && this.RightExpression.ExactMatch(item);

    /// <inheritdoc />
    public bool PartialMatch(Item item) =>
        this.LeftExpression.PartialMatch(item) && this.RightExpression.PartialMatch(item);
}