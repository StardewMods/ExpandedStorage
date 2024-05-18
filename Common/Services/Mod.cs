namespace StardewMods.Common.Services;

/// <summary>Static wrapper for common mod services.</summary>
internal class Mod
{
    private static Mod instance = null!;

    private readonly IManifest manifest;

    public Mod(IManifest manifest)
    {
        Mod.instance = this;
        this.manifest = manifest;
    }

    /// <summary>Gets the unique id for this mod.</summary>
    public static string Id => Mod.instance.manifest.UniqueID;

    /// <summary>Gets the manifest for this mod.</summary>
    public static IManifest Manifest => Mod.instance.manifest;
}