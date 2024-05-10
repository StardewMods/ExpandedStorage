namespace StardewMods.Common.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.Menus;

/// <summary>Represents a custom component.</summary>
public interface IComponent
{
    /// <summary>Gets the clickable component.</summary>
    ClickableComponent Component { get; }

    /// <summary>Check if the cursor position is within the bounds of the component.</summary>
    /// <param name="position">The cursor position.</param>
    /// <returns><c>true</c> if the cursor is within the bounds; otherwise, <c>false</c>.</returns>
    bool Contains(Vector2 position);

    /// <summary>Draws the component.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    void Draw(SpriteBatch spriteBatch);

    /// <summary>Try to handles an input event.</summary>
    /// <param name="eventArgs">The event args.</param>
    /// <returns><c>true</c> if the input was handled; otherwise, <c>false</c>.</returns>
    bool TryHandleInput(ButtonPressedEventArgs eventArgs);

    /// <summary>Update the component based on the cursor position.</summary>
    /// <param name="x">The mouse x.</param>
    /// <param name="y">The mouse y.</param>
    void Update(int x, int y);
}