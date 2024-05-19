namespace StardewMods.Common.Services;

/// <inheritdoc />
[SuppressMessage("Naming", "CA1716", Justification = "Reviewed")]
public abstract class Mod : StardewModdingAPI.Mod
{
    private static Mod instance = null!;

    /// <summary>Gets the unique id for this mod.</summary>
    public static string Id => Mod.instance.ModManifest.UniqueID;

    /// <summary>Gets the manifest for this mod.</summary>
    public static IManifest Manifest => Mod.instance.ModManifest;

    /// <inheritdoc />
    public sealed override void Entry(IModHelper helper)
    {
        Mod.instance = this;
        this.Init();
    }

    /// <summary>Initialize the mod.</summary>
    protected abstract void Init();
}