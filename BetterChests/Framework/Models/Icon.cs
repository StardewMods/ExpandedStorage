namespace StardewMods.BetterChests.Framework.Models;

using Microsoft.Xna.Framework;

/// <summary>Represents an icon on a sprite sheet.</summary>
internal sealed class Icon
{
    /// <summary>Gets or sets the id of the tab icon.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the path to the tab icon.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets the area of the tab icon.</summary>
    public Rectangle Area { get; set; } = Rectangle.Empty;
}