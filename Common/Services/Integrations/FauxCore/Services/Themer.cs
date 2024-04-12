namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class Themer : IThemeHelper
{
    private readonly Lazy<IThemeHelper> themeHelper;

    /// <summary>Initializes a new instance of the <see cref="Themer"/> class.</summary>
    /// <param name="fauxCoreIntegration">Dependency used for FauxCore integration.</param>
    public Themer(FauxCoreIntegration fauxCoreIntegration) =>
        this.themeHelper = new Lazy<IThemeHelper>(() => fauxCoreIntegration.Api!.CreateThemeService());

    /// <inheritdoc />
    public IManagedTexture AddAsset(string path, IRawTextureData data) => this.themeHelper.Value.AddAsset(path, data);
}