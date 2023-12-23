namespace StardewMods.CustomBush;

using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Mod config data for Custom Bush.</summary>
internal sealed class ModConfig : IConfigWithLogLevel
{
    /// <inheritdoc />
    public SimpleLogLevel LogLevel { get; set; } = SimpleLogLevel.Less;
}
