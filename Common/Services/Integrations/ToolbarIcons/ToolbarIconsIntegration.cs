namespace StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
internal sealed class ToolbarIconsIntegration : ModIntegration<IToolbarIconsApi>
{
    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ToolbarIconsIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "furyx639.ToolbarIcons";
}