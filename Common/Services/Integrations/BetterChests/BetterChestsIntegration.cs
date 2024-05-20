namespace StardewMods.Common.Services.Integrations.BetterChests;

/// <inheritdoc />
internal sealed class BetterChestsIntegration : ModIntegration<IBetterChestsApi>
{
    /// <summary>Initializes a new instance of the <see cref="BetterChestsIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public BetterChestsIntegration(IModRegistry modRegistry)
        : base(modRegistry) { }

    /// <inheritdoc />
    public override string UniqueId => "furyx639.BetterChests";

    /// <inheritdoc/>
    public override ISemanticVersion Version { get; } = new SemanticVersion(2, 19, 0);
}