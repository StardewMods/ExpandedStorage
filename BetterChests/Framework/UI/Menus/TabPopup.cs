namespace StardewMods.BetterChests.Framework.UI.Menus;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Menus;

internal sealed class TabPopup : BaseMenu
{
    public TabPopup(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int? x = null,
        int? y = null,
        int? width = null,
        int? height = null,
        bool showUpperRightCloseButton = false)
        : base(inputHelper, x, y, width, height, showUpperRightCloseButton)
    {
        // var selectIcon = new SelectIcon(
        //     inputHelper,
        //     reflectionHelper,
        //     iconRegistry.GetIcons(),)
    }
}