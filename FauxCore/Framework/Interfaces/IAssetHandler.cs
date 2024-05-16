namespace StardewMods.FauxCore.Framework.Interfaces;

using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
public interface IAssetHandler
{
    /// <summary>Generate the texture of a button with the given icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <returns>Returns the button texture.</returns>
    Texture2D CreateButtonTexture(IIcon icon);

    /// <summary>Get the texture of an icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <returns>Returns the texture.</returns>
    Texture2D GetTexture(IIcon icon);
}