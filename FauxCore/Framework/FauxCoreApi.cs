namespace StardewMods.FauxCore.Framework;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Services;

/// <inheritdoc />
public sealed class FauxCoreApi : IFauxCoreApi
{
    private readonly IAssetHandler assetHandler;
    private readonly Func<IModConfig> getConfig;
    private readonly IModInfo modInfo;

    private IIconRegistry? iconRegistry;
    private ILog? log;
    private IPatchManager? patchManager;

    /// <summary>Initializes a new instance of the <see cref="FauxCoreApi" /> class.</summary>
    /// <param name="modInfo">Dependency used for accessing mod info.</param>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="getConfig">Dependency used for managing config data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public FauxCoreApi(
        IModInfo modInfo,
        IAssetHandler assetHandler,
        IExpressionHandler expressionHandler,
        Func<IModConfig> getConfig,
        IThemeHelper themeHelper)
    {
        this.modInfo = modInfo;
        this.assetHandler = assetHandler;
        this.ExpressionHandler = expressionHandler;
        this.getConfig = getConfig;
        this.ThemeHelper = themeHelper;
    }

    /// <inheritdoc />
    public IExpressionHandler ExpressionHandler { get; }

    /// <inheritdoc />
    public IIconRegistry IconRegistry =>
        this.iconRegistry ??= new IconRegistry(
            this.assetHandler,
            this.Log ?? throw new InvalidOperationException("Log is not set."),
            this.modInfo.Manifest);

    /// <inheritdoc />
    public ILog Log =>
        this.log ??= new Log(
            this.getConfig,
            this.Monitor ?? throw new InvalidOperationException("Monitor is not set."));

    /// <inheritdoc />
    public IPatchManager PatchManager =>
        this.patchManager ??= new PatchManager(
            this.Log ?? throw new InvalidOperationException("Log is not set."),
            this.modInfo.Manifest);

    /// <inheritdoc />
    public IThemeHelper ThemeHelper { get; }

    /// <inheritdoc />
    public IMonitor? Monitor { get; set; }
}