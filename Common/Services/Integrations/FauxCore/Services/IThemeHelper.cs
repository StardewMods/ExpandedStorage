namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Handles palette swaps for theme compatibility.</summary>
public interface IThemeHelper
{
    /// <summary>Adds a new asset to theme helper using the provided texture data and asset name.</summary>
    /// <param name="path">The game content path for the asset.</param>
    /// <param name="data">The raw texture data for the asset.</param>
    public void AddAsset(string path, IRawTextureData data);

    /// <summary>Retrieves a managed texture asset based on the given path.</summary>
    /// <param name="path">The path to the asset.</param>
    /// <returns>Returns the managed texture.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no asset is found for the given path.</exception>
    public IManagedTexture RequireAsset(string path);

    /// <summary>Attempt to retrieve a managed texture from theme helper for the given path.</summary>
    /// <param name="path">The game content path to the asset.</param>
    /// <param name="texture">When this method returns, contains the managed texture if found; otherwise, null.</param>
    /// <returns><c>true</c> if the managed texture exists; otherwise, <c>false</c>.</returns>
    public bool TryGetAsset(string path, [NotNullWhen(true)] out IManagedTexture? texture);
}