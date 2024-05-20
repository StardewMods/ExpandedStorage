namespace StardewMods.ToolbarIcons.Framework.Interfaces;

using StardewModdingAPI.Utilities;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Mod config data for Toolbar Icons.</summary>
internal interface IModConfig
{
    /// <summary>Gets a value containing the toolbar icons.</summary>
    public List<ToolbarIcon> Icons { get; }

    /// <summary>Gets the size that icons will be scaled to.</summary>
    public float Scale { get; }

    /// <summary>Gets the key to toggle icons on or off.</summary>
    public KeybindList ToggleKey { get; }

    /// <summary>Gets a value indicating whether icons should be visible.</summary>
    public bool Visible { get; }
}