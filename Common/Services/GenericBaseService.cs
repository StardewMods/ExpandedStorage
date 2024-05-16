namespace StardewMods.Common.Services;

using StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal abstract class GenericBaseService<TService> : BaseService
    where TService : class
{
    /// <summary>Initializes a new instance of the <see cref="GenericBaseService{TService}" /> class.</summary>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    protected GenericBaseService(ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.Id = typeof(TService).Name;
        this.UniqueId = this.ModId + "/" + this.Id;
        this.Prefix = this.ModId + "-" + this.Id + "-";
    }

    /// <summary>Gets a unique id for this service.</summary>
    public string Id { get; }

    /// <summary>Gets a globally unique prefix for this service.</summary>
    public string Prefix { get; }

    /// <summary>Gets a globally unique id for this service.</summary>
    public string UniqueId { get; }
}