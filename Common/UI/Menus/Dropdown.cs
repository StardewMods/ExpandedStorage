namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Dropdown menu for selecting an item from a list of values.</summary>
internal class Dropdown : BaseMenu
{
    private readonly Action<string> callback;

    /// <summary>Initializes a new instance of the <see cref="Dropdown" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="items">The list of values to select from.</param>
    /// <param name="callback">A method that is called when an option is selected.</param>
    /// <param name="minWidth">The minimum width.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    public Dropdown(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        ClickableComponent anchor,
        IReadOnlyCollection<KeyValuePair<string, string>> items,
        Action<string> callback,
        int minWidth = 0,
        int maxItems = int.MaxValue)
        : base(inputHelper)
    {
        this.callback = callback;
        var selectOption = new SelectOption(
            inputHelper,
            reflectionHelper,
            items,
            this.OnSelect,
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            minWidth,
            maxItems);

        this.AddSubMenu(selectOption);
        this.Resize(selectOption.width + 16, selectOption.height + 16);
        this.MoveTo(anchor.bounds.Left, anchor.bounds.Bottom);
        if (this.xPositionOnScreen + this.width > Game1.uiViewport.Width)
        {
            this.MoveTo(anchor.bounds.Right - this.width, this.yPositionOnScreen);
        }

        if (this.yPositionOnScreen + this.height > Game1.uiViewport.Height)
        {
            this.MoveTo(this.xPositionOnScreen, anchor.bounds.Top - this.height + 16);
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch) { }

    /// <inheritdoc />
    protected override bool TryLeftClick(int x, int y)
    {
        this.exitThisMenuNoSound();
        return false;
    }

    private void OnSelect(string? value)
    {
        this.callback(value ?? string.Empty);
        this.exitThisMenuNoSound();
    }
}