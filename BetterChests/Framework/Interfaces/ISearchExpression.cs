namespace StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Represents a search expression.</summary>
internal interface ISearchExpression
{
    /// <summary>Determines if the expression exactly matches the specified item.</summary>
    /// <param name="item">The item to match.</param>
    /// <returns>true if the items matches; otherwise, false.</returns>
    bool ExactMatch(Item item);

    /// <summary>Determines if the expression partially matches the specified item.</summary>
    /// <param name="item">The item to be match.</param>
    /// <returns>true if the item matches; otherwise, false.</returns>
    bool PartialMatch(Item item);
}