namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>The event arguments before a container is sorted.</summary>
internal sealed class ContainerSortingEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ContainerSortingEventArgs" /> class.</summary>
    /// <param name="container">The container being sorted.</param>
    public ContainerSortingEventArgs(IStorageContainer container)
    {
        this.Container = container;
        this.OriginalItems = [.. container.Items];
    }

    /// <summary>Gets the container being sorted.</summary>
    public IStorageContainer Container { get; }

    /// <summary>Gets the original list of items.</summary>
    public IReadOnlyList<Item> OriginalItems { get; }
}