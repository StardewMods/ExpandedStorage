namespace StardewMods.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Represents an icon on a sprite sheet.</summary>
public interface IIcon
{
    /// <summary>Gets the icon source area.</summary>
    public Rectangle Area { get; }

    /// <summary>Gets the icon id.</summary>
    public string Id { get; }

    /// <summary>Gets the icon texture path.</summary>
    public string Path { get; }

    /// <summary>Gets the icon texture.</summary>
    public Texture2D Texture { get; }

    /// <summary>Gets a component with the icon.</summary>
    /// <param name="style">The style of the component</param>
    /// <returns>Returns a new button.</returns>
    public ClickableTextureComponent GetComponent(ComponentStyle style);
}