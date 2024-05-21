namespace StardewMods.Common.Interfaces;

/// <summary>Factory service for creating Api instances.</summary>
internal interface IApiFactory
{
    /// <summary>Creates a new instance of the Api.</summary>
    /// <param name="modInfo">Dependency used for accessing mod info.</param>
    /// <returns>Returns the api instance.</returns>
    public object CreateApi(IModInfo modInfo);
}