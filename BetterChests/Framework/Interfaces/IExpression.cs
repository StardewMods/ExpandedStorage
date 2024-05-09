namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a search expression.</summary>
internal interface IExpression : IComparer<Item>
{
    /// <summary>Determines whether the expression matches the specified item.</summary>
    /// <param name="item">The item to match against.</param>
    /// <returns><c>true</c> if the item matches; otherwise, <c>false</c>.</returns>
    bool Matches(Item item);

    /// <summary>Determines whether the expression matches the specified container.</summary>
    /// <param name="container">The container to match against.</param>
    /// <returns><c>true</c> if the container matches; otherwise, <c>false</c>.</returns>
    bool Matches(IStorageContainer container);
}