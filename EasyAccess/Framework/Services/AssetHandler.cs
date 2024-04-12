namespace StardewMods.EasyAccess.Framework.Services;

using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly Lazy<IManagedTexture> icon;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(ILog log, IManifest manifest, IModContentHelper modContentHelper, IThemeHelper themeHelper)
        : base(log, manifest) =>
        this.icon = new Lazy<IManagedTexture>(
            () => themeHelper.AddAsset(
                this.ModId + "/Icons",
                modContentHelper.Load<IRawTextureData>("assets/icons.png")));

    /// <summary>Gets the managed icon texture.</summary>
    public IManagedTexture IconTexture => this.icon.Value;
}