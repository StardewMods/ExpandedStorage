namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewValley.Menus;

/// <summary>Represents the event arguments for changes in the inventory menu.</summary>
internal sealed class InventoryMenuChangedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="InventoryMenuChangedEventArgs" /> class.</summary>
    /// <param name="parent">The parent menu.</param>
    /// <param name="top">The top menu.</param>
    /// <param name="bottom">The bottom menu.</param>
    public InventoryMenuChangedEventArgs(IClickableMenu? parent, IClickableMenu? top, IClickableMenu? bottom)
    {
        this.Parent = parent;
        this.Top = top;
        this.Bottom = bottom;
    }

    /// <summary>Gets the bottom menu.</summary>
    public IClickableMenu? Bottom { get; }

    /// <summary>Gets the parent menu.</summary>
    public IClickableMenu? Parent { get; }

    /// <summary>Gets the top menu.</summary>
    public IClickableMenu? Top { get; }
}