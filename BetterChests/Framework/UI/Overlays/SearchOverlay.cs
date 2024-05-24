namespace StardewMods.BetterChests.Framework.UI.Overlays;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;

/// <summary>Menu for searching for chests which contain specific items.</summary>
internal sealed class SearchOverlay : BaseMenu
{
    private readonly TextField textField;

    /// <summary>Initializes a new instance of the <see cref="SearchOverlay" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="getMethod">The function that gets the current search text.</param>
    /// <param name="setMethod">The action that sets the search text.</param>
    public SearchOverlay(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        Func<string> getMethod,
        Action<string> setMethod)
        : base(inputHelper)
    {
        var searchBarWidth = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);
        var origin = Utility.getTopLeftPositionForCenteringOnScreen(searchBarWidth, 48);

        this.textField = new TextField(
            this.Input,
            reflectionHelper,
            (int)origin.X,
            Game1.tileSize,
            searchBarWidth,
            getMethod,
            setMethod)
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
    public override bool TryLeftClick(Point cursor)
    {
        this.textField.TryLeftClick(cursor);
        if (this.textField.Selected)
        {
            return false;
        }

        this.textField.Selected = false;
        this.exitThisMenuNoSound();
        return true;
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        this.textField.TryRightClick(cursor);
        if (this.textField.Selected)
        {
            return false;
        }

        this.textField.Selected = false;
        this.exitThisMenuNoSound();
        return true;
    }
}