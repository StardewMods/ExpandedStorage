#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.ProjectFluent;
#else

namespace StardewMods.Common.Services.Integrations.ProjectFluent;
#endif

/// <inheritdoc />
internal sealed class ProjectFluentIntegration : ModIntegration<IProjectFluentApi>
{
    /// <summary>Initializes a new instance of the <see cref="ProjectFluentIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ProjectFluentIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "Shockah.ProjectFluent";

    /// <inheritdoc />
    public override ISemanticVersion Version { get; } = new SemanticVersion(2, 0, 0);
}