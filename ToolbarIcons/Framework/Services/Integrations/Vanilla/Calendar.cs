namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class Calendar : IVanillaIntegration
{
    /// <inheritdoc />
    public string HoverText => I18n.Button_Calendar();

    /// <inheritdoc />
    public string Icon => InternalIcon.Calendar.ToStringFast();

    /// <inheritdoc />
    public void DoAction() => Game1.activeClickableMenu = new Billboard();
}