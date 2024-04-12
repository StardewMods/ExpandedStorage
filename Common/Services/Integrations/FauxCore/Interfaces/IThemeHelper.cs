namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Handles palette swaps for theme compatibility.</summary>
public interface IThemeHelper
{
    /// <summary>Adds a new asset to the asset manager using the provided texture data and asset name.</summary>
    /// <param name="path">The game content path for the asset.</param>
    /// <param name="data">The raw texture data for the asset.</param>
    /// <returns>Returns the managed texture.</returns>
    public IManagedTexture AddAsset(string path, IRawTextureData data);
}