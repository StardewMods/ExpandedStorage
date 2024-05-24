namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>Represents the event arguments for changes in the inventory menu.</summary>
internal sealed class InventoryMenuChangedEventArgs : EventArgs
{
    private readonly List<BaseComponent>? components;

    /// <summary>Initializes a new instance of the <see cref="InventoryMenuChangedEventArgs" /> class.</summary>
    /// <param name="components">The list of custom components.</param>
    /// <param name="parent">The parent menu.</param>
    /// <param name="top">The top menu.</param>
    /// <param name="bottom">The bottom menu.</param>
    public InventoryMenuChangedEventArgs(
        List<BaseComponent>? components,
        IClickableMenu? parent,
        IClickableMenu? top,
        IClickableMenu? bottom)
    {
        this.components = components;
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

    /// <summary>Add a custom component to the menu.</summary>
    /// <param name="component">The component to add.</param>
    public void AddComponent(BaseComponent component) => this.components?.Add(component);
}