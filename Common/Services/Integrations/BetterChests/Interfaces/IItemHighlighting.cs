namespace StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

using StardewValley.Menus;

/// <summary>The event arguments before an item is highlighted.</summary>
public interface IItemHighlighting
{
    /// <summary>Gets the container with the item being highlighted.</summary>
    public IStorageContainer Container { get; }

    /// <summary>Gets the item being highlighted.</summary>
    public Item Item { get; }

    /// <summary>Gets a value indicating whether the item is highlighted.</summary>
    public bool IsHighlighted { get; }

    /// <summary>UnHighlight the item.</summary>
    public void UnHighlight();
}