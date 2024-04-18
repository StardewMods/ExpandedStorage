namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterChests.Interfaces.IItemHighlighting" />
public class ItemHighlightingEventArgs : EventArgs, IItemHighlighting
{
    /// <summary>Initializes a new instance of the <see cref="ItemHighlightingEventArgs" /> class.</summary>
    /// <param name="container">The container with the item being highlighted.</param>
    /// <param name="item">The item being highlighted.</param>
    public ItemHighlightingEventArgs(IStorageContainer container, Item item)
    {
        this.Container = container;
        this.Item = item;
    }

    /// <inheritdoc />
    public IStorageContainer Container { get; }

    /// <inheritdoc />
    public Item Item { get; }

    /// <inheritdoc />
    public bool IsHighlighted { get; private set; } = true;

    /// <inheritdoc />
    public void UnHighlight() => this.IsHighlighted = false;
}