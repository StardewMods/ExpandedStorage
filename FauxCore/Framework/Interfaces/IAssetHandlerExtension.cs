namespace StardewMods.FauxCore.Framework.Interfaces;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Extended Asset Handler methods for FauxCore.</summary>
public interface IAssetHandlerExtension : IAssetHandler
{
    /// <summary>Adds a button asset for the icon.</summary>
    /// <param name="icon">The icon.</param>
    public void AddAsset(IIcon icon);
}