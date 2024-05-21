namespace StardewMods.Common.Services;

using StardewMods.Common.Interfaces;

/// <inheritdoc />
[SuppressMessage("Naming", "CA1716", Justification = "Reviewed")]
internal abstract class Mod : StardewModdingAPI.Mod
{
    private static Mod instance = null!;

    /// <summary>Gets the container.</summary>
    private Container container = null!;

    /// <summary>Gets the unique id for this mod.</summary>
    public static string Id => Mod.instance.ModManifest.UniqueID;

    /// <summary>Gets the manifest for this mod.</summary>
    public static IManifest Manifest => Mod.instance.ModManifest;

    /// <summary>Gets the unique prefix for this mod.</summary>
    public static string Prefix => Mod.Id + "/";

    /// <inheritdoc />
    public sealed override void Entry(IModHelper helper)
    {
        // Init
        Mod.instance = this;
        this.container = new Container();

        // Configuration
        this.container.RegisterInstance(this.Helper);
        this.container.RegisterInstance(this.ModManifest);
        this.container.RegisterInstance(this.Monitor);
        this.container.RegisterInstance(this.Helper.ConsoleCommands);
        this.container.RegisterInstance(this.Helper.Data);
        this.container.RegisterInstance(this.Helper.Events);
        this.container.RegisterInstance(this.Helper.GameContent);
        this.container.RegisterInstance(this.Helper.Input);
        this.container.RegisterInstance(this.Helper.ModContent);
        this.container.RegisterInstance(this.Helper.ModRegistry);
        this.container.RegisterInstance(this.Helper.Reflection);
        this.container.RegisterInstance(this.Helper.Translation);
        this.container.RegisterSingleton<Log>();

        this.Init(this.container);
        this.container.Verify();
    }

    /// <summary>Create an api instance for the requesting mod.</summary>
    /// <param name="mod">The requesting mod.</param>
    /// <returns>Returns an api instance.</returns>
    protected object CreateApi(IModInfo mod) => this.container.GetInstance<IApiFactory>().CreateApi(mod);

    /// <summary>Initialize the mod.</summary>
    /// <param name="container">The container.</param>
    protected abstract void Init(Container container);
}