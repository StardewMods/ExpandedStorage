#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Interfaces.UI;

using Microsoft.Xna.Framework;
using StardewValley.Menus;

#else
namespace StardewMods.Common.Interfaces.UI;

using Microsoft.Xna.Framework;
using StardewValley.Menus;
#endif

/// <summary>Represents a custom menu.</summary>
public interface ICustomMenu
{
    /// <summary>Gets the menu dimensions.</summary>
    Point Dimensions { get; }

    /// <summary>Gets the parent menu.</summary>
    IClickableMenu? Parent { get; }

    /// <summary>Gets the sub menus.</summary>
    IEnumerable<IClickableMenu> SubMenus { get; }

    /// <summary>Gets or sets the menu bounds.</summary>
    Rectangle Bounds { get; set; }

    /// <summary>Gets or sets the hover text.</summary>
    string? HoverText { get; set; }

    /// <summary>Moves the menu to the specified position.</summary>
    /// <param name="position">The position.</param>
    void MoveTo(Point position);

    /// <summary>Resize the menu to the specified dimensions.</summary>
    /// <param name="dimensions">The menu dimensions.</param>
    void Resize(Point dimensions);
}