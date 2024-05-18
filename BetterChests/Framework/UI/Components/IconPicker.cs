namespace StardewMods.BetterChests.Framework.UI.Components;

using StardewMods.Common.UI.Menus;

internal sealed class IconPicker : BaseMenu
{
    public IconPicker(
        IInputHelper inputHelper,
        int? x = null,
        int? y = null,
        int? width = null,
        int? height = null,
        bool showUpperRightCloseButton = false)
        : base(inputHelper, x, y, width, height, showUpperRightCloseButton) { }
}