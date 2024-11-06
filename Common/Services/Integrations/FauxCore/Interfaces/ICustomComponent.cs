#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#else
namespace StardewMods.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

/// <summary>Represents a custom component.</summary>
public interface ICustomComponent
{
    /// <summary>Event raised when the component is clicked.</summary>
    event EventHandler<IClicked> Clicked;

    /// <summary>Gets the component bounds.</summary>
    Rectangle Bounds { get; }

    /// <summary>Gets the color.</summary>
    Color Color { get; }

    /// <summary>Gets the hover text.</summary>
    string? HoverText { get; }

    /// <summary>Gets the component position.</summary>
    Point Location { get; }

    /// <summary>Gets the parent menu.</summary>
    ICustomMenu? Parent { get; }

    /// <summary>Gets the component size.</summary>
    Point Size { get; }

    /// <summary>Draws the component.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    /// <param name="cursor">The mouse position.</param>
    /// <param name="offset">The offset.</param>
    void Draw(SpriteBatch spriteBatch, Point cursor, Point offset);

    /// <summary>Moves the component to the specified position.</summary>
    /// <param name="location">The position to move to.</param>
    /// <returns>Returns the component.</returns>
    ICustomComponent MoveTo(Point location);

    /// <summary>Resize the component to the specified dimensions.</summary>
    /// <param name="size">The component dimensions.</param>
    /// <returns>Returns the component.</returns>
    ICustomComponent ResizeTo(Point size);

    /// <summary>Sets the color.</summary>
    /// <param name="value">The color value.</param>
    /// <returns>Returns the component.</returns>
    ICustomComponent SetColor(Color value);

    /// <summary>Sets the hover text.</summary>
    /// <param name="value">The hover text value.</param>
    /// <returns>Returns the component.</returns>
    ICustomComponent SetHoverText(string? value);

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