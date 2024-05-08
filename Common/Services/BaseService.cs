namespace StardewMods.Common.Services;

using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>This abstract class serves as the base for all service classes.</summary>
internal abstract class BaseService
{
    /// <summary>Initializes a new instance of the <see cref="BaseService" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    protected BaseService(ILog log, IManifest manifest)
    {
        this.Log = log;
        this.ModId = manifest.UniqueID;
    }

    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    protected ILog Log { get; }

    /// <summary>Gets the unique id for this mod.</summary>
    protected string ModId { get; }
}