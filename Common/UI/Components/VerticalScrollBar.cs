namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Represents a scrollbar with up/down arrows.</summary>
internal sealed class VerticalScrollBar : BaseComponent
{
    private readonly ClickableTextureComponent arrowDown;
    private readonly ClickableTextureComponent arrowUp;
    private readonly Func<int> getMax;
    private readonly Func<int> getMethod;
    private readonly Func<int> getMin;
    private readonly Func<int> getStepSize;
    private readonly ClickableTextureComponent grabber;
    private readonly Action<int> setMethod;

    private Rectangle runner;

    /// <summary>Initializes a new instance of the <see cref="VerticalScrollBar" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="x">The x-coordinate of the scroll bar.</param>
    /// <param name="y">The y-coordinate of the scroll bar.</param>
    /// <param name="height">The height of the scroll bar.</param>
    /// <param name="getMethod">A function that gets the current value.</param>
    /// <param name="setMethod">An action that sets the current value.</param>
    /// <param name="getMin">A function that gets the minimum value.</param>
    /// <param name="getMax">A function that gets the maximum value.</param>
    /// <param name="getStepSize">The step size for arrows or scroll wheel.</param>
    /// <param name="name">The name of the scroll bar.</param>
    public VerticalScrollBar(
        IInputHelper inputHelper,
        int x,
        int y,
        int height,
        Func<int> getMethod,
        Action<int> setMethod,
        Func<int> getMin,
        Func<int> getMax,
        Func<int>? getStepSize = null,
        string name = "ScrollBar")
        : base(inputHelper, x, y, 48, height, name)
    {
        this.getMethod = getMethod;
        this.setMethod = setMethod;
        this.getMin = getMin;
        this.getMax = getMax;
        this.getStepSize = getStepSize ?? VerticalScrollBar.GetDefaultStepSize;

        this.arrowUp = new ClickableTextureComponent(
            new Rectangle(x, y, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom);

        this.arrowDown = new ClickableTextureComponent(
            new Rectangle(x, y + height - (12 * Game1.pixelZoom), 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom);

        this.runner = new Rectangle(x + 12, y + (12 * Game1.pixelZoom) + 4, 24, height - (24 * Game1.pixelZoom) - 12);
        this.grabber = new ClickableTextureComponent(
            new Rectangle(x + 12, this.runner.Y, 24, 40),
            Game1.mouseCursors,
            new Rectangle(435, 463, 6, 10),
            Game1.pixelZoom);
    }

    /// <summary>Gets the maximum value of the source.</summary>
    public int MaxValue => this.getMax();

    /// <summary>Gets the minimum value of the source.</summary>
    public int MinValue => this.getMin();

    /// <summary>Gets the step size.</summary>
    public int StepSize => this.getStepSize();

    /// <summary>Gets a value indicating whether the scroll bar is currently active.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets or sets the the value that the scrollbar is tied to.</summary>
    public int SourceValue
    {
        get => this.getMethod();
        set => this.setMethod(Math.Min(this.MaxValue, Math.Max(this.MinValue, value)));
    }

    /// <summary>Gets or sets the percentage value of the scroll bar.</summary>
    public float Value { get; set; }

    /// <summary>Draws the search overlay to the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public override void Draw(SpriteBatch spriteBatch)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(403, 383, 6, 6),
            this.runner.X,
            this.runner.Y,
            this.runner.Width,
            this.runner.Height,
            Color.White,
            Game1.pixelZoom);

        if (this.SourceValue > this.MinValue || this.SourceValue < this.MaxValue)
        {
            this.grabber.draw(spriteBatch);
        }

        this.arrowUp.draw(spriteBatch, this.SourceValue > this.MinValue ? Color.White : Color.Black * 0.35f, 1f);
        this.arrowDown.draw(spriteBatch, this.SourceValue < this.MaxValue ? Color.White : Color.Black * 0.35f, 1f);
    }

    /// <inheritdoc />
    public override void MoveTo(int x, int y)
    {
        base.MoveTo(x, y);
        this.arrowUp.bounds.X = this.bounds.X;
        this.arrowUp.bounds.Y = this.bounds.Y;
        this.arrowDown.bounds.X = this.bounds.X;
        this.arrowDown.bounds.Y = this.bounds.Y + this.bounds.Height - (12 * Game1.pixelZoom);
        this.runner = new Rectangle(
            this.bounds.X + 12,
            this.bounds.Y + (12 * Game1.pixelZoom) + 4,
            24,
            this.bounds.Height - (24 * Game1.pixelZoom) - 12);

        this.grabber.bounds.X = this.bounds.X + 12;
        this.SetScrollPosition();
    }

    /// <inheritdoc />
    public override void Resize(int width, int height)
    {
        base.Resize(width, height);
        this.arrowDown.bounds.Y = this.bounds.Y + this.bounds.Height - (12 * Game1.pixelZoom);
        this.runner = new Rectangle(
            this.bounds.X + 12,
            this.bounds.Y + (12 * Game1.pixelZoom) + 4,
            24,
            this.bounds.Height - (24 * Game1.pixelZoom) - 12);
    }

    /// <inheritdoc />
    public override bool TryLeftClick(int mouseX, int mouseY)
    {
        if (this.grabber.containsPoint(mouseX, mouseY))
        {
            this.IsActive = true;
            return true;
        }

        if (this.runner.Contains(mouseX, mouseY))
        {
            this.Value = (float)(mouseY - this.runner.Y) / this.runner.Height;
            this.SetScrollPosition();
            return true;
        }

        if (this.arrowUp.containsPoint(mouseX, mouseY) && this.SourceValue > this.MinValue)
        {
            this.arrowUp.scale = 3.5f;
            Game1.playSound("shwip");
            this.SourceValue -= this.StepSize;
            this.SetScrollPosition();
            return true;
        }

        if (this.arrowDown.containsPoint(mouseX, mouseY) && this.SourceValue < this.MaxValue)
        {
            this.arrowDown.scale = 3.5f;
            Game1.playSound("shwip");
            this.SourceValue += this.StepSize;
            this.SetScrollPosition();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool TryScroll(int direction)
    {
        var initialValue = this.SourceValue;

        // Scroll down
        if (direction < 0)
        {
            this.SourceValue += this.StepSize;
        }

        // Scroll up
        if (direction > 0)
        {
            this.SourceValue -= this.StepSize;
        }

        this.SetScrollPosition();
        if (this.SourceValue != initialValue)
        {
            Game1.playSound("shiny4");
            return true;
        }

        return false;
    }

    /// <summary>Updates the search bar based on the mouse position.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse position.</param>
    /// <param name="mouseY">The y-coordinate of the mouse position.</param>
    public override void Update(int mouseX, int mouseY)
    {
        if (!this.IsActive)
        {
            if (this.SourceValue > this.MinValue)
            {
                this.arrowUp.tryHover(mouseX, mouseY);
            }

            if (this.SourceValue < this.MaxValue)
            {
                this.arrowDown.tryHover(mouseX, mouseY);
            }

            return;
        }

        this.Value = Math.Min(1, Math.Max(0, (float)(mouseY - this.runner.Y) / this.runner.Height));
        var initialY = this.grabber.bounds.Y;
        this.SourceValue = (int)((this.Value * (this.MaxValue - this.MinValue)) + this.MinValue);
        this.SetScrollPosition();
        if (initialY != this.grabber.bounds.Y)
        {
            Game1.playSound("shiny4");
        }

        this.IsActive = this.Input.IsDown(SButton.MouseLeft) || this.Input.IsSuppressed(SButton.MouseLeft);
    }

    private static int GetDefaultStepSize() => 1;

    private void SetScrollPosition()
    {
        this.Value = (float)Math.Round((float)(this.SourceValue - this.MinValue) / (this.MaxValue - this.MinValue), 2);
        this.grabber.bounds.Y = Math.Max(
            this.runner.Y,
            Math.Min(
                this.runner.Y + (int)((this.runner.Height - this.grabber.bounds.Height) * this.Value),
                this.runner.Bottom - this.grabber.bounds.Height));
    }
}