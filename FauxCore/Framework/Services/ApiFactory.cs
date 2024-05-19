namespace StardewMods.FauxCore.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ApiFactory : IApiFactory<IFauxCoreApi>
{
    private readonly IAssetHandler assetHandler;
    private readonly IExpressionHandler expressionHandler;
    private readonly IModConfig modConfig;
    private readonly IThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="ApiFactory" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public ApiFactory(
        IAssetHandler assetHandler,
        IExpressionHandler expressionHandler,
        IModConfig modConfig,
        IThemeHelper themeHelper)
    {
        this.assetHandler = assetHandler;
        this.expressionHandler = expressionHandler;
        this.modConfig = modConfig;
        this.themeHelper = themeHelper;
    }

    /// <inheritdoc />
    public IFauxCoreApi CreateApi(IModInfo modInfo) =>
        new FauxCoreApi(modInfo, this.assetHandler, this.expressionHandler, this.modConfig, this.themeHelper);
}