namespace StardewMods.CustomBush.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.CustomBush;

/// <inheritdoc />
internal sealed class ApiFactory : IApiFactory<ICustomBushApi>
{
    private readonly AssetHandler assetHandler;
    private readonly ModPatches modPatches;

    /// <summary>Initializes a new instance of the <see cref="ApiFactory" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="modPatches">Dependency for managing custom bushes.</param>
    public ApiFactory(AssetHandler assetHandler, ModPatches modPatches)
    {
        this.assetHandler = assetHandler;
        this.modPatches = modPatches;
    }

    /// <inheritdoc />
    public ICustomBushApi CreateApi(IModInfo modInfo) => new CustomBushApi(this.assetHandler, modInfo, this.modPatches);
}