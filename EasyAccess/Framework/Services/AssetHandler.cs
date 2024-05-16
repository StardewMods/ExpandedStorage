namespace StardewMods.EasyAccess.Framework.Services;

using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(ILog log, IManifest manifest, IModContentHelper modContentHelper, IThemeHelper themeHelper)
        : base(log, manifest)
    {
        this.themeHelper = themeHelper;
        themeHelper.AddAsset(this.ModId + "/Icons", modContentHelper.Load<IRawTextureData>("assets/icons.png"));
    }

    /// <summary>Gets the managed icon texture.</summary>
    public IManagedTexture Icon =>
        this.themeHelper.TryGetAsset(this.ModId + "/Icons", out var texture)
            ? texture
            : throw new InvalidOperationException("The icon texture is not available.");
}