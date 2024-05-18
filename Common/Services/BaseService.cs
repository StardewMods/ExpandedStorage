namespace StardewMods.Common.Services;

/// <summary>This abstract class serves as the base for all service classes.</summary>
internal abstract class BaseService
{
    /// <summary>Initializes a new instance of the <see cref="BaseService" /> class.</summary>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    protected BaseService(IManifest manifest) => this.ModId = manifest.UniqueID;

    /// <summary>Gets the unique id for this mod.</summary>
    protected string ModId { get; }
}