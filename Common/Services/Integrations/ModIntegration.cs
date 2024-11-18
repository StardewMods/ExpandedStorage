#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations;

using StardewMods.FauxCore.Common.Interfaces;

#else

namespace StardewMods.Common.Services.Integrations;

using StardewMods.Common.Interfaces;

#endif

/// <summary>Provides an integration point for using external mods' APIs.</summary>
/// <typeparam name="T">Interface for the external mod's API.</typeparam>
internal abstract class ModIntegration<T> : IModIntegration
    where T : class
{
    private readonly Lazy<T?> modApi;

    /// <summary>Initializes a new instance of the <see cref="ModIntegration{T}" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    internal ModIntegration(IModRegistry modRegistry)
    {
        this.ModRegistry = modRegistry;
        this.modApi = new Lazy<T?>(() => this.ModRegistry.GetApi<T>(this.UniqueId));
    }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(ModIntegration<T>.Api), nameof(ModIntegration<T>.ModInfo))]
    public bool IsLoaded =>
        this.ModRegistry.IsLoaded(this.UniqueId)
        && (this.Version is null || this.ModInfo?.Manifest.Version.IsOlderThan(this.Version) != true);

    /// <inheritdoc />
    public IModInfo? ModInfo => this.ModRegistry.Get(this.UniqueId);

    /// <inheritdoc />
    public abstract string UniqueId { get; }

    /// <inheritdoc />
    public virtual ISemanticVersion? Version => null;

    /// <summary>Gets the Mod's API through SMAPI's standard interface.</summary>
    protected internal T? Api => this.IsLoaded ? this.modApi.Value : default(T?);

    private IModRegistry ModRegistry { get; }
}