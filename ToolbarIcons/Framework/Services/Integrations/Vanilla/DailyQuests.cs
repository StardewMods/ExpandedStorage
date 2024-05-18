namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class DailyQuests : IVanillaIntegration
{
    /// <inheritdoc />
    public string HoverText => I18n.Button_DailyQuests();

    /// <inheritdoc />
    public string Icon => InternalIcon.DailyQuests.ToStringFast();

    /// <inheritdoc />
    public void DoAction() => Game1.activeClickableMenu = new Billboard(true);
}