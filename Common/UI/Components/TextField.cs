namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Represents a search overlay control that allows the user to input text.</summary>
internal sealed class TextField : BaseComponent
{
    private const int CountdownTimer = 20;

    private readonly Func<string> getMethod;
    private readonly ClickableTextureComponent icon;
    private readonly Action<string> setMethod;
    private readonly TextBox textBox;
    private string previousText;
    private int timeout;

    /// <summary>Initializes a new instance of the <see cref="TextField" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="x">The text field x-coordinate.</param>
    /// <param name="y">The text field y-coordinate.</param>
    /// <param name="width">The text field width.</param>
    /// <param name="getMethod">A function that gets the current value.</param>
    /// <param name="setMethod">An action that sets the current value.</param>
    /// <param name="name">The text field name.</param>
    public TextField(
        IInputHelper inputHelper,
        int x,
        int y,
        int width,
        Func<string> getMethod,
        Action<string> setMethod,
        string name = "TextField")
        : base(inputHelper, x, y, width, 48, name)
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
            limitWidth = false,
            Text = this.previousText,
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
    public override void Draw(SpriteBatch spriteBatch) => this.textBox.Draw(spriteBatch, false);

    /// <summary>Reset the value of the text box.</summary>
    public void Reset() => this.textBox.Text = this.Text;

    /// <inheritdoc />
    public override bool TryLeftClick(int mouseX, int mouseY)
    {
        this.Selected = this.bounds.Contains(mouseX, mouseY);
        return this.Selected;
    }

    /// <inheritdoc />
    public override bool TryRightClick(int mouseX, int mouseY)
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
    public override void Update(int mouseX, int mouseY)
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

        this.timeout = TextField.CountdownTimer;
        this.previousText = this.textBox.Text;
    }
}