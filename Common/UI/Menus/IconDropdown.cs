#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Dropdown menu with icons.</summary>
internal sealed class IconDropdown : BaseMenu
{
    private EventHandler<IIcon?>? iconSelected;

    /// <summary>Initializes a new instance of the <see cref="IconDropdown" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="icons">The list of values to select from.</param>
    /// <param name="rows">This rows of icons to display.</param>
    /// <param name="columns">The columns of icons to display.</param>
    /// <param name="getHoverText">A function which returns the hover text for an icon.</param>
    /// <param name="scale">The icon scale.</param>
    /// <param name="spacing">The spacing between icons.</param>
    public IconDropdown(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        ClickableComponent anchor,
        IEnumerable<IIcon> icons,
        int rows,
        int columns,
        SelectIcon.GetHoverText? getHoverText = null,
        float scale = 3f,
        int spacing = 8)
        : base(inputHelper)
    {
        var selectIcon = new SelectIcon(
            inputHelper,
            reflectionHelper,
            icons,
            rows,
            columns,
            getHoverText,
            scale,
            spacing,
            this.xPositionOnScreen,
            this.yPositionOnScreen);

        selectIcon.SelectionChanged += this.OnSelectionChanged;
        this.AddSubMenu(selectIcon);
        this.Resize(new Point(selectIcon.width + 16, selectIcon.height + 16));
        this.MoveTo(new Point(anchor.bounds.Left, anchor.bounds.Bottom));
        if (this.xPositionOnScreen + this.width > Game1.uiViewport.Width)
        {
            this.MoveTo(new Point(anchor.bounds.Right - this.width, this.yPositionOnScreen));
        }

        if (this.yPositionOnScreen + this.height > Game1.uiViewport.Height)
        {
            this.MoveTo(new Point(this.xPositionOnScreen, anchor.bounds.Top - this.height + 16));
        }
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> IconSelected
    {
        add => this.iconSelected += value;
        remove => this.iconSelected -= value;
    }

    /// <inheritdoc />
    protected override bool TryLeftClick(Point cursor)
    {
        this.exitThisMenuNoSound();
        return false;
    }

    private void OnSelectionChanged(object? sender, IIcon? icon)
    {
        if (icon is null)
        {
            return;
        }

        this.iconSelected?.InvokeAll(this, icon);
        this.exitThisMenuNoSound();
    }
}