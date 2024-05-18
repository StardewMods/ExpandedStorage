namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class CjbItemSpawner : IActionIntegration
{
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="CjbItemSpawner" /> class.</summary>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public CjbItemSpawner(IReflectionHelper reflectionHelper) => this.reflectionHelper = reflectionHelper;

    /// <inheritdoc />
    public string HoverText => I18n.Button_ItemSpawner();

    /// <inheritdoc />
    public string Icon => VanillaIcon.Gift.ToStringFast();

    /// <inheritdoc />
    public string ModId => "CJBok.ItemSpawner";

    /// <inheritdoc />
    public Action? GetAction(IMod mod)
    {
        var buildMenu = this.reflectionHelper.GetMethod(mod, "BuildMenu", false);
        return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
    }
}