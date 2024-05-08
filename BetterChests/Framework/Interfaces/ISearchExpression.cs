namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a search expression.</summary>
internal interface ISearchExpression
{
    /// <summary>Determines if the expression exactly matches the specified item.</summary>
    /// <param name="item">The item to matched.</param>
    /// <returns>true if the items matches; otherwise, false.</returns>
    bool ExactMatch(Item item);

    /// <summary>Determines if the expression partially matches the specified item.</summary>
    /// <param name="item">The item to be matched.</param>
    /// <returns>true if the item matches; otherwise, false.</returns>
    bool PartialMatch(Item item);

    /// <summary>Determines if the expression exactly matches the specified container.</summary>
    /// <param name="container">The container to search.</param>
    /// <returns>true if the container matches; otherwise, false.</returns>
    bool ExactMatch(IStorageContainer container);

    /// <summary>Determines if the expression partially matches the specified container.</summary>
    /// <param name="container">The container to search.</param>
    /// <returns>true if the container matches; otherwise, false.</returns>
    bool PartialMatch(IStorageContainer container);
}