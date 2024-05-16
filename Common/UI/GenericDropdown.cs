namespace StardewMods.Common.UI;

using StardewValley.Menus;

/// <inheritdoc />
internal class GenericDropdown<TKey> : BaseDropdown
    where TKey : struct
{
    private readonly Action<TKey?> callback;
    private readonly List<(TKey Key, string Value)> items;

    /// <summary>Initializes a new instance of the <see cref="GenericDropdown{TKey}" /> class.</summary>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="items">The list of values to display.</param>
    /// <param name="callback">The action to call when a value is selected.</param>
    /// <param name="maxItems">The maximum number of items to display at once.</param>
    public GenericDropdown(
        ClickableComponent anchor,
        List<(TKey Key, string Value)> items,
        Action<TKey?> callback,
        int maxItems = int.MaxValue)
        : base(anchor, items.Select(item => item.Value).ToList(), maxItems)
    {
        this.items = items;
        this.callback = callback;
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        var selectedIndex = this.LeftClick(x, y);
        if (selectedIndex == -1)
        {
            this.callback(null);
            this.exitThisMenuNoSound();
            return;
        }

        var (key, _) = this.items.ElementAtOrDefault(selectedIndex);
        this.callback(key);
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true) { }
}