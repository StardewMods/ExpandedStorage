namespace StardewMods.ToolbarIcons.Framework.Models;

using StardewMods.ToolbarIcons.Framework.Enums;

/// <summary>Data model for Toolbar Icons integration.</summary>
internal sealed class IntegrationData
{
    /// <summary>Gets or sets additional data depending on the integration type.</summary>
    public string ExtraData { get; set; } = string.Empty;

    /// <summary>Gets or sets the hover text.</summary>
    public string HoverText { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique id for the mod integration.</summary>
    public string ModId { get; set; } = string.Empty;

    /// <summary>Gets or sets the integration type.</summary>
    public IntegrationType Type { get; set; }
}