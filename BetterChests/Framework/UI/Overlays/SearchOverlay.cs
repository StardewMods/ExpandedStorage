namespace StardewMods.BetterChests.Framework.UI.Overlays;

using Microsoft.Xna.Framework.Input;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;

/// <summary>Menu for searching for chests which contain specific items.</summary>
internal sealed class SearchOverlay : BaseMenu
{
    private readonly TextField textField;

    /// <summary>Initializes a new instance of the <see cref="SearchOverlay" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="getMethod">The function that gets the current search text.</param>
    /// <param name="setMethod">The action that sets the search text.</param>
    public SearchOverlay(IInputHelper inputHelper, Func<string> getMethod, Action<string> setMethod)
        : base(inputHelper)
    {
        var searchBarWidth = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);
        var origin = Utility.getTopLeftPositionForCenteringOnScreen(searchBarWidth, 48);

        this.textField =
            new TextField(this.Input, (int)origin.X, Game1.tileSize, searchBarWidth, getMethod, setMethod)
            {
                Selected = true,
            };

        this.allClickableComponents.Add(this.textField);
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key is Keys.Enter or Keys.Escape)
        {
            this.exitThisMenuNoSound();
        }
    }

    /// <inheritdoc />
    protected override bool TryLeftClick(int x, int y)
    {
        this.textField.TryLeftClick(x, y);
        if (this.textField.Selected)
        {
            return false;
        }

        this.textField.Selected = false;
        this.exitThisMenuNoSound();
        return true;
    }

    /// <inheritdoc />
    protected override bool TryRightClick(int x, int y)
    {
        this.textField.TryRightClick(x, y);
        if (this.textField.Selected)
        {
            return false;
        }

        this.textField.Selected = false;
        this.exitThisMenuNoSound();
        return true;
    }
}