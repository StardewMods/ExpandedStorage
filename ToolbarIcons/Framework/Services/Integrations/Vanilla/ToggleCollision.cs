namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ToggleCollision : IVanillaIntegration
{
    /// <inheritdoc />
    public string HoverText => I18n.Button_NoClip();

    /// <inheritdoc />
    public string Icon => InternalIcon.ToggleCollision.ToStringFast();

    /// <inheritdoc />
    public void DoAction()
    {
        if (Context.IsPlayerFree || (Context.IsWorldReady && Game1.eventUp))
        {
            Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
        }
    }
}