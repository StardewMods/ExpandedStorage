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

    /// <summary>Gets the unique prefix for this mod.</summary>
    public static string Prefix => Mod.Id + "/";

    /// <summary>Gets the container.</summary>
    protected Container Container { get; private set; } = null!;

    /// <inheritdoc />
    public sealed override void Entry(IModHelper helper)
    {
        // Init
        Mod.instance = this;
        this.Container = new Container();

        // Configuration
        this.Container.RegisterInstance(this.Helper);
        this.Container.RegisterInstance(this.ModManifest);
        this.Container.RegisterInstance(this.Monitor);
        this.Container.RegisterInstance(this.Helper.ConsoleCommands);
        this.Container.RegisterInstance(this.Helper.Data);
        this.Container.RegisterInstance(this.Helper.Events);
        this.Container.RegisterInstance(this.Helper.GameContent);
        this.Container.RegisterInstance(this.Helper.Input);
        this.Container.RegisterInstance(this.Helper.ModContent);
        this.Container.RegisterInstance(this.Helper.ModRegistry);
        this.Container.RegisterInstance(this.Helper.Reflection);
        this.Container.RegisterInstance(this.Helper.Translation);

        this.Init();
        this.Container.Verify();
    }

    /// <summary>Initialize the mod.</summary>
    protected abstract void Init();
}