#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Interfaces.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#else
namespace StardewMods.Common.Interfaces.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

/// <summary>Represents a custom component.</summary>
public interface ICustomComponent
{
    /// <summary>Gets the hover text.</summary>
    string? HoverText { get; }

    /// <summary>Gets the component position.</summary>
    Point Position { get; }

    /// <summary>Draws the component.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    /// <param name="cursor">The mouse position.</param>
    /// <param name="offset">The offset.</param>
    void Draw(SpriteBatch spriteBatch, Point cursor, Point offset);

    /// <summary>Moves the component to the specified position.</summary>
    /// <param name="cursor">The position to move to.</param>
    void MoveTo(Point cursor);

    /// <summary>Resize the component to the specified dimensions.</summary>
    /// <param name="dimensions">The component dimensions.</param>
    void Resize(Point dimensions);

    /// <summary>Attempts to left-click the component based at the cursor position.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if the click was handled; otherwise, <c>false</c>.</returns>
    bool TryLeftClick(Point cursor);

    /// <summary>Attempts to left-click the component based at the cursor position.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if the click was handled; otherwise, <c>false</c>.</returns>
    bool TryRightClick(Point cursor);

    /// <summary>Attempts to scroll the component.</summary>
    /// <param name="direction">The scroll direction.</param>
    /// <returns><c>true</c> if the scroll was handled; otherwise, <c>false</c>.</returns>
    bool TryScroll(int direction);

    /// <summary>Update the component based on the cursor position.</summary>
    /// <param name="cursor">The mouse position.</param>
    void Update(Point cursor);
}