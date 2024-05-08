namespace StardewMods.Common.Services.Integrations;

using StardewMods.Common.Interfaces;

/// <summary>Provides an integration point for using external mods' APIs.</summary>
/// <typeparam name="T">Interface for the external mod's API.</typeparam>
internal abstract class ModIntegration<T> : IModIntegration
    where T : class
{
    private readonly Lazy<T?> modApi;

    /// <summary>Initializes a new instance of the <see cref="ModIntegration{T}" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="modUniqueId">The unique id of the external mod.</param>
    /// <param name="modVersion">The minimum supported version.</param>
    internal ModIntegration(IModRegistry modRegistry, string modUniqueId, string modVersion = "")
    {
        this.ModRegistry = modRegistry;
        this.UniqueId = modUniqueId;
        this.Version = string.IsNullOrWhiteSpace(modVersion) ? null : modVersion;
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
    public string UniqueId { get; }

    /// <inheritdoc />
    public string? Version { get; }

    /// <summary>Gets the Mod's API through SMAPI's standard interface.</summary>
    protected internal T? Api => this.IsLoaded ? this.modApi.Value : default(T?);

    private IModRegistry ModRegistry { get; }
}