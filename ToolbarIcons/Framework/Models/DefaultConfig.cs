namespace StardewMods.ToolbarIcons.Framework.Models;

using StardewModdingAPI.Utilities;
using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public List<ToolbarIcon> Icons { get; set; } = [];

    /// <inheritdoc />
    public bool PlaySound { get; set; } = true;

    /// <inheritdoc />
    public float Scale { get; set; } = 2;

    /// <inheritdoc />
    public bool ShowTooltip { get; set; } = true;

    /// <inheritdoc />
    public KeybindList ToggleKey { get; set; } = new(new Keybind(SButton.LeftControl, SButton.Tab));

    /// <inheritdoc />
    public bool Visible { get; set; } = true;
}