namespace StardewMods.Common.Interfaces;

/// <summary>Factory service for creating Api instances.</summary>
/// <typeparam name="TApi">The api class.</typeparam>
internal interface IApiFactory<out TApi>
{
    /// <summary>Creates a new instance of the Api.</summary>
    /// <param name="modInfo">Dependency used for accessing mod info.</param>
    /// <returns>Returns the api instance.</returns>
    public TApi CreateApi(IModInfo modInfo);
}