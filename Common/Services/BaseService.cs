#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services;
#else
namespace StardewMods.Common.Services;
#endif

/// <summary>This abstract class serves as the base for all service classes.</summary>
internal abstract class BaseService<TService>
    where TService : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseService{TService}" /> class.</summary>
    protected BaseService()
    {
        this.Id = typeof(TService).Name;
        this.UniqueId = Mod.Id + "/" + this.Id;
        this.Prefix = Mod.Id + "-" + this.Id + "-";
    }

    /// <summary>Gets a unique id for this service.</summary>
    public string Id { get; }

    /// <summary>Gets a globally unique prefix for this service.</summary>
    public string Prefix { get; }

    /// <summary>Gets a globally unique id for this service.</summary>
    public string UniqueId { get; }
}