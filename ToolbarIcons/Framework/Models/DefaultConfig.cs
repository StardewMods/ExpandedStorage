namespace StardewMods.ToolbarIcons.Framework.Models;

using StardewModdingAPI.Utilities;
using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public List<ToolbarIcon> Icons { get; set; } = [];

    /// <inheritdoc />
    public float Scale { get; set; } = 2;

    /// <inheritdoc />
    public KeybindList ToggleKey { get; set; } = new(new Keybind(SButton.LeftControl, SButton.Tab));

    /// <inheritdoc />
    public bool Visible { get; set; } = true;
}