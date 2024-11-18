#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else

namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#endif

/// <summary>Dropdown menu for selecting an item from a list of values.</summary>
/// <typeparam name="TItem">The item type.</typeparam>
internal sealed class Dropdown<TItem> : BaseMenu
{
    private EventHandler<TItem?>? optionSelected;

    /// <summary>Initializes a new instance of the <see cref="Dropdown{TItem}" /> class.</summary>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="items">The list of values to select from.</param>
    /// <param name="getValue">A function which returns a string from the item.</param>
    /// <param name="minWidth">The minimum width.</param>
    /// <param name="maxWidth">The maximum width.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    public Dropdown(
        ClickableComponent anchor,
        IEnumerable<TItem> items,
        Func<TItem, string>? getValue = null,
        int minWidth = 0,
        int maxWidth = int.MaxValue,
        int maxItems = int.MaxValue)
    {
        var selectOption = new SelectOption<TItem>(
            items,
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            getValue,
            minWidth,
            maxWidth,
            maxItems);

        selectOption.SelectionChanged += this.OnSelectionChanged;

        var offset = anchor is ICustomComponent
        {
            Parent: IFramedMenu parent,
        }
            ? parent.CurrentOffset
            : Point.Zero;

        this
            .AddSubMenu(selectOption)
            .ResizeTo(new Point(selectOption.width + 16, selectOption.height + 16))
            .MoveTo(new Point(anchor.bounds.Left - offset.X, anchor.bounds.Bottom - offset.Y));

        if (this.xPositionOnScreen + this.width > Game1.uiViewport.Width)
        {
            this.MoveTo(new Point(anchor.bounds.Right - this.width - offset.X, this.yPositionOnScreen - offset.Y));
        }

        if (this.yPositionOnScreen + this.height > Game1.uiViewport.Height)
        {
            this.MoveTo(new Point(this.xPositionOnScreen - offset.X, anchor.bounds.Top - this.height + 16 - offset.Y));
        }
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<TItem?> OptionSelected
    {
        add => this.optionSelected += value;
        remove => this.optionSelected -= value;
    }

    /// <inheritdoc />
    public override void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    { }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        this.exitThisMenuNoSound();
        return false;
    }

    private void OnSelectionChanged(object? sender, TItem? item)
    {
        if (item is null)
        {
            return;
        }

        this.optionSelected?.InvokeAll(this, item);
        this.exitThisMenuNoSound();
    }
}