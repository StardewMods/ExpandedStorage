#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#else

namespace StardewMods.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#endif

/// <summary>Represents a custom menu.</summary>
public interface ICustomMenu
{
    /// <summary>Gets the menu bounds.</summary>
    Rectangle Bounds { get; }

    /// <summary>Gets the menu dimensions.</summary>
    Point Dimensions { get; }

    /// <summary>Gets the hover text.</summary>
    string? HoverText { get; }

    /// <summary>Gets the parent menu.</summary>
    IClickableMenu? Parent { get; }

    /// <summary>Gets the sub menus.</summary>
    IEnumerable<IClickableMenu> SubMenus { get; }

    /// <summary>Adds a submenu to the current menu.</summary>
    /// <param name="subMenu">The submenu to add.</param>
    /// <returns>Returns the menu.</returns>
    ICustomMenu AddSubMenu(IClickableMenu subMenu);

    /// <summary>Draw to the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    void Draw(SpriteBatch spriteBatch, Point cursor);

    /// <summary>Draw over the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    void DrawOver(SpriteBatch spriteBatch, Point cursor);

    /// <summary>Draw under the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    void DrawUnder(SpriteBatch spriteBatch, Point cursor);

    /// <summary>Moves the menu to the specified position.</summary>
    /// <param name="position">The position.</param>
    /// <returns>Returns the menu.</returns>
    ICustomMenu MoveTo(Point position);

    /// <summary>Resize the menu to the specified dimensions.</summary>
    /// <param name="size">The menu dimensions.</param>
    /// <returns>Returns the menu.</returns>
    ICustomMenu ResizeTo(Point size);

    /// <summary>Sets the hover text.</summary>
    /// <param name="value">The hover text value.</param>
    /// <returns>Returns the menu.</returns>
    ICustomMenu SetHoverText(string? value);

    /// <summary>Try to perform a hover.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if hover was handled; otherwise, <c>false</c>.</returns>
    bool TryHover(Point cursor);

    /// <summary>Try to perform a left-click.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if left click was handled; otherwise, <c>false</c>.</returns>
    bool TryLeftClick(Point cursor);

    /// <summary>Try to perform a right-click.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if right click was handled; otherwise, <c>false</c>.</returns>
    bool TryRightClick(Point cursor);

    /// <summary>Try to perform a scroll.</summary>
    /// <param name="direction">The direction of the scroll.</param>
    /// <returns><c>true</c> if scroll was handled; otherwise, <c>false</c>.</returns>
    bool TryScroll(int direction);

    /// <summary>Performs an update.</summary>
    /// <param name="cursor">The mouse position.</param>
    void Update(Point cursor);
}