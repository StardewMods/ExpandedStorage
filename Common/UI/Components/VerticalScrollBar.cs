#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

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
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
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
        IReflectionHelper reflectionHelper,
        int x,
        int y,
        int height,
        Func<int> getMethod,
        Action<int> setMethod,
        Func<int> getMin,
        Func<int> getMax,
        Func<int>? getStepSize = null,
        string name = "ScrollBar")
        : base(inputHelper, reflectionHelper, x, y, 48, height, name)
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

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(403, 383, 6, 6),
            this.runner.X + offset.X,
            this.runner.Y + offset.Y,
            this.runner.Width,
            this.runner.Height,
            Color.White,
            Game1.pixelZoom);

        if (this.SourceValue > this.MinValue || this.SourceValue < this.MaxValue)
        {
            this.grabber.draw(spriteBatch, Color.White, 1f, 0, offset.X, offset.Y);
        }

        this.arrowUp.draw(
            spriteBatch,
            this.SourceValue > this.MinValue ? Color.White : Color.Black * 0.35f,
            1f,
            0,
            offset.X,
            offset.Y);

        this.arrowDown.draw(
            spriteBatch,
            this.SourceValue < this.MaxValue ? Color.White : Color.Black * 0.35f,
            1f,
            0,
            offset.X,
            offset.Y);
    }

    /// <inheritdoc />
    public override ICustomComponent MoveTo(Point location)
    {
        base.MoveTo(location);
        this.arrowUp.bounds.Location = this.bounds.Location;
        this.arrowDown.bounds.X = this.bounds.X;
        this.arrowDown.bounds.Y = this.bounds.Y + this.bounds.Height - (12 * Game1.pixelZoom);
        this.runner = new Rectangle(
            this.bounds.X + 12,
            this.bounds.Y + (12 * Game1.pixelZoom) + 4,
            24,
            this.bounds.Height - (24 * Game1.pixelZoom) - 12);

        this.grabber.bounds.X = this.bounds.X + 12;
        this.SetScrollPosition();
        return this;
    }

    /// <inheritdoc />
    public override ICustomComponent ResizeTo(Point size)
    {
        base.ResizeTo(size);
        this.arrowDown.bounds.Y = this.bounds.Y + this.bounds.Height - (12 * Game1.pixelZoom);
        this.runner = new Rectangle(
            this.bounds.X + 12,
            this.bounds.Y + (12 * Game1.pixelZoom) + 4,
            24,
            this.bounds.Height - (24 * Game1.pixelZoom) - 12);

        return this;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.grabber.bounds.Contains(cursor))
        {
            this.IsActive = true;
            return true;
        }

        if (this.runner.Contains(cursor))
        {
            this.Value = (float)(cursor.Y - this.runner.Y) / this.runner.Height;
            this.SetScrollPosition();
            return true;
        }

        if (this.arrowUp.bounds.Contains(cursor) && this.SourceValue > this.MinValue)
        {
            this.arrowUp.scale = 3.5f;
            Game1.playSound("shwip");
            this.SourceValue -= this.StepSize;
            this.SetScrollPosition();
            return true;
        }

        if (this.arrowDown.bounds.Contains(cursor) && this.SourceValue < this.MaxValue)
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

    /// <inheritdoc />
    public override void Update(Point cursor)
    {
        if (!this.IsActive)
        {
            if (this.SourceValue > this.MinValue)
            {
                this.arrowUp.tryHover(cursor.X, cursor.Y);
            }

            if (this.SourceValue < this.MaxValue)
            {
                this.arrowDown.tryHover(cursor.X, cursor.Y);
            }

            return;
        }

        this.Value = Math.Clamp((float)(cursor.Y - this.runner.Y) / this.runner.Height, 0, 1);
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