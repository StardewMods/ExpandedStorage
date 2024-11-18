namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>The event arguments before items are displayed.</summary>
public class ItemsDisplayingEventArgs : EventArgs
{
    private IList<Item> items;

    /// <summary>Initializes a new instance of the <see cref="ItemsDisplayingEventArgs" /> class.</summary>
    /// <param name="container">The container with the items being displayed.</param>
    public ItemsDisplayingEventArgs(IStorageContainer container)
    {
        this.Container = container;
        this.items = [.. container.Items];
    }

    /// <summary>Gets the container with the items being displayed.</summary>
    public IStorageContainer Container { get; }

    /// <summary>Gets the items being displayed.</summary>
    public IEnumerable<Item> Items => this.items;

    /// <summary>Updates the collection of items using the specified operation.</summary>
    /// <param name="operation">A function that takes a collection of items and returns a modified collection.</param>
    public void Edit(Func<IEnumerable<Item>, IEnumerable<Item>> operation) =>
        this.items = [.. operation.Invoke(this.items)];
}