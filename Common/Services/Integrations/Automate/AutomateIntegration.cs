#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.Automate;
#else

namespace StardewMods.Common.Services.Integrations.Automate;
#endif

/// <inheritdoc />
internal sealed class AutomateIntegration : ModIntegration<IAutomateApi>
{
    /// <summary>Initializes a new instance of the <see cref="AutomateIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public AutomateIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "Pathoschild.Automate";
}