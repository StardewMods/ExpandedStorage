#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
#endif

/// <summary>Represents a search overlay control that allows the user to input text.</summary>
internal sealed class TextField : BaseComponent
{
    private const int CountdownTimer = 20;

    private readonly Func<string> getMethod;
    private readonly Action<string> setMethod;
    private readonly TextBox textBox;
    private string previousText;
    private int timeout;

    /// <summary>Initializes a new instance of the <see cref="TextField" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="x">The text field x-coordinate.</param>
    /// <param name="y">The text field y-coordinate.</param>
    /// <param name="width">The text field width.</param>
    /// <param name="getMethod">A function that gets the current value.</param>
    /// <param name="setMethod">An action that sets the current value.</param>
    /// <param name="name">The text field name.</param>
    public TextField(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int x,
        int y,
        int width,
        Func<string> getMethod,
        Action<string> setMethod,
        string name = "TextField")
        : base(inputHelper, reflectionHelper, x, y, width, 48, name)
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

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        this.textBox.X = this.bounds.X + offset.X;
        this.textBox.Y = this.bounds.Y + offset.Y;
        this.textBox.Draw(spriteBatch, false);
    }

    /// <summary>Reset the value of the text box.</summary>
    public void Reset() => this.textBox.Text = this.Text;

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        this.Selected = this.bounds.Contains(cursor);
        return this.Selected;
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        if (!this.bounds.Contains(cursor))
        {
            this.Selected = false;
            return false;
        }

        this.Selected = true;
        this.textBox.Text = string.Empty;
        return this.Selected;
    }

    /// <inheritdoc />
    public override void Update(Point cursor)
    {
        this.textBox.Hover(cursor.X, cursor.Y);
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