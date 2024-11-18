#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.Profiler;
#else

namespace StardewMods.Common.Services.Integrations.Profiler;
#endif

/// <inheritdoc />
internal sealed class ProfilerIntegration : ModIntegration<IProfilerApi>
{
    /// <summary>Initializes a new instance of the <see cref="ProfilerIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ProfilerIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "SinZ.Profiler";

    /// <inheritdoc />
    public override ISemanticVersion Version { get; } = new SemanticVersion(2, 0, 0);
}