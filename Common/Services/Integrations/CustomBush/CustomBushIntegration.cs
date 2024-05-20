namespace StardewMods.Common.Services.Integrations.CustomBush;

/// <inheritdoc />
internal sealed class CustomBushIntegration : ModIntegration<ICustomBushApi>
{
    /// <summary>Initializes a new instance of the <see cref="CustomBushIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public CustomBushIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "furyx639.CustomBush";

    /// <inheritdoc/>
    public override ISemanticVersion Version { get; } = new SemanticVersion(1, 2, 2);
}