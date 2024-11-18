#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.BetterCrafting;
#else

namespace StardewMods.Common.Services.Integrations.BetterCrafting;
#endif

/// <inheritdoc />
internal sealed class BetterCraftingIntegration : ModIntegration<IBetterCrafting>
{
    /// <summary>Initializes a new instance of the <see cref="BetterCraftingIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public BetterCraftingIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "leclair.bettercrafting";

    /// <inheritdoc />
    public override ISemanticVersion Version { get; } = new SemanticVersion(1, 2, 0);
}