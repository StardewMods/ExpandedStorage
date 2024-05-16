namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;

/// <summary>Represents an icon on a sprite sheet.</summary>
internal sealed class IconData
{
    /// <summary>Gets or sets the area of the icon.</summary>
    public Rectangle Area { get; set; } = Rectangle.Empty;

    /// <summary>Gets or sets the path to the icon.</summary>
    public string Path { get; set; } = string.Empty;
}