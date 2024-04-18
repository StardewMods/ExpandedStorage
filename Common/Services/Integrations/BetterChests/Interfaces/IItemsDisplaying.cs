namespace StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <summary>The event arguments before items are displayed.</summary>
public interface IItemsDisplaying
{
    /// <summary>Gets the container with the items being displayed.</summary>
    public IStorageContainer Container { get; }

/// <summary>Gets the items being displayed.</summary>
    public IEnumerable<Item> Items { get; }

    /// <summary>Updates the collection of items using the specified operation.</summary>
    /// <param name="operation">A function that takes a collection of items and returns a modified collection.</param>
    public void Edit(Func<IEnumerable<Item>, IEnumerable<Item>> operation);
}