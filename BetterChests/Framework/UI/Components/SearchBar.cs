namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Represents a search overlay control that allows the user to input text.</summary>
internal sealed class SearchBar : ClickableComponent
{
    private const int CountdownTimer = 20;

    private readonly Func<string> getMethod;
    private readonly ClickableTextureComponent icon;
    private readonly Action<string> setMethod;
    private readonly TextBox textBox;
    private string previousText;
    private int timeout;

    /// <summary>Initializes a new instance of the <see cref="SearchBar" /> class.</summary>
    /// <param name="x">The x-coordinate of the search bar.</param>
    /// <param name="y">The y-coordinate of the search bar.</param>
    /// <param name="width">The width of the search bar.</param>
    /// <param name="getMethod">The function that gets the current search text.</param>
    /// <param name="setMethod">The action that sets the search text.</param>
    public SearchBar(int x, int y, int width, Func<string> getMethod, Action<string> setMethod)
        : base(new Rectangle(x, y, width, 48), "SearchBar")
    {
        this.previousText = getMethod();
        this.getMethod = getMethod;
        this.setMethod = setMethod;
        var texture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
        this.textBox = new TextBox(texture, null, Game1.smallFont, Game1.textColor)
        {
            X = this.bounds.X,
            Y = this.bounds.Y,
            Width = this.bounds.Width,
            Text = this.previousText,
            limitWidth = false,
        };

        this.icon = new ClickableTextureComponent(
            new Rectangle(this.bounds.X + this.textBox.Width - 38, this.bounds.Y + 6, 32, 32),
            Game1.mouseCursors,
            new Rectangle(80, 0, 13, 13),
            2.5f);
    }

    /// <summary>Gets or sets a value indicating whether the search bar is currently selected.</summary>
    public bool Selected
    {
        get => this.textBox.Selected;
        set => this.textBox.Selected = value;
    }

    private string Text
    {
        get => this.getMethod();
        set => this.setMethod(value);
    }

    /// <summary>Draws the search overlay to the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        this.textBox.Draw(spriteBatch);
        this.icon.draw(spriteBatch);
    }

    /// <summary>Performs a left click at the specified coordinates on the screen.</summary>
    /// <param name="mouseX">The X-coordinate of the mouse click.</param>
    /// <param name="mouseY">The Y-coordinate of the mouse click.</param>
    /// <returns><c>true</c> if the search bar was clicked; otherwise, <c>false</c>.</returns>
    public bool LeftClick(int mouseX, int mouseY)
    {
        this.Selected = this.bounds.Contains(mouseX, mouseY);
        return this.Selected;
    }

    /// <summary>Reset the value of the text box.</summary>
    public void Reset() => this.textBox.Text = this.Text;

    /// <summary>Performs a right click at the specified coordinates on the screen.</summary>
    /// <param name="mouseX">The X-coordinate of the mouse click.</param>
    /// <param name="mouseY">The Y-coordinate of the mouse click.</param>
    /// <returns><c>true</c> if the search bar was clicked; otherwise, <c>false</c>.</returns>
    public bool RightClick(int mouseX, int mouseY)
    {
        if (!this.bounds.Contains(mouseX, mouseY))
        {
            this.Selected = false;
            return false;
        }

        this.Selected = true;
        this.textBox.Text = string.Empty;
        return this.Selected;
    }

    /// <summary>Updates the search bar based on the mouse position.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse position.</param>
    /// <param name="mouseY">The y-coordinate of the mouse position.</param>
    public void Update(int mouseX, int mouseY)
    {
        this.textBox.Hover(mouseX, mouseY);
        if (this.timeout > 0 && --this.timeout == 0 && this.Text != this.textBox.Text)
        {
            this.Text = this.textBox.Text;
        }

        if (this.textBox.Text.Equals(this.previousText, StringComparison.Ordinal))
        {
            return;
        }

        this.timeout = SearchBar.CountdownTimer;
        this.previousText = this.textBox.Text;
    }
}