#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
#endif

/// <summary>Framed menu with vertical scrolling.</summary>
internal abstract class FramedMenu : BaseMenu, IFramedMenu
{
    private readonly IReflectionHelper reflectionHelper;
    private readonly VerticalScrollBar scrollBar;

    private Point maxOffset;
    private Point offset;

    /// <summary>Initializes a new instance of the <see cref="FramedMenu" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="x">The x-position of the menu.</param>
    /// <param name="y">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    /// <param name="showUpperRightCloseButton">A value indicating whether to show the right close button.</param>
    protected FramedMenu(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int? x = null,
        int? y = null,
        int? width = null,
        int? height = null,
        bool showUpperRightCloseButton = false)
        : base(inputHelper, x, y, width, height, showUpperRightCloseButton)
    {
        this.reflectionHelper = reflectionHelper;
        this.maxOffset = new Point(-1, -1);
        this.scrollBar = new VerticalScrollBar(
            this.Input,
            reflectionHelper,
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + 4,
            this.height,
            () => this.offset.Y,
            value =>
            {
                this.offset.Y = value;
            },
            () => 0,
            () => this.MaxOffset.Y,
            () => this.StepSize);
    }

    /// <summary>Gets or sets the y-offset.</summary>
    public Point CurrentOffset => this.offset;

    /// <summary>Gets the frame.</summary>
    public virtual Rectangle Frame => this.Bounds;

    /// <inheritdoc />
    public Point MaxOffset => this.maxOffset;

    /// <summary>Gets the step size for scrolling.</summary>
    public virtual int StepSize => 1;

    /// <inheritdoc />
    public sealed override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        var sortModeReflected = this.reflectionHelper.GetField<SpriteSortMode>(spriteBatch, "_sortMode", false);
        var sortModeOriginal = sortModeReflected?.GetValue() ?? SpriteSortMode.Deferred;

        var blendStateReflected = this.reflectionHelper.GetField<BlendState>(spriteBatch, "_blendState", false);
        var blendStateOriginal = blendStateReflected?.GetValue();

        var samplerStateReflected = this.reflectionHelper.GetField<SamplerState>(spriteBatch, "_samplerState", false);
        var samplerStateOriginal = samplerStateReflected?.GetValue();

        var depthStencilStateReflected =
            this.reflectionHelper.GetField<DepthStencilState>(spriteBatch, "_depthStencilState", false);

        var depthStencilStateOriginal = depthStencilStateReflected?.GetValue();

        var rasterizerStateReflected =
            this.reflectionHelper.GetField<RasterizerState>(spriteBatch, "_rasterizerState", false);

        var rasterizerStateOriginal = rasterizerStateReflected?.GetValue();

        var effectReflected = this.reflectionHelper.GetField<Effect>(spriteBatch, "_effect", false);
        var effectOriginal = effectReflected?.GetValue();

        var scissorOriginal = spriteBatch.GraphicsDevice.ScissorRectangle;

        var rasterizerState = new RasterizerState { ScissorTestEnable = true };
        if (rasterizerStateOriginal is not null)
        {
            rasterizerState.CullMode = rasterizerStateOriginal.CullMode;
            rasterizerState.FillMode = rasterizerStateOriginal.FillMode;
            rasterizerState.DepthBias = rasterizerStateOriginal.DepthBias;
            rasterizerState.MultiSampleAntiAlias = rasterizerStateOriginal.MultiSampleAntiAlias;
            rasterizerState.SlopeScaleDepthBias = rasterizerStateOriginal.SlopeScaleDepthBias;
            rasterizerState.DepthClipEnable = rasterizerStateOriginal.DepthClipEnable;
        }

        spriteBatch.End();

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            blendStateOriginal,
            samplerStateOriginal,
            depthStencilStateOriginal,
            rasterizerState,
            effectOriginal);

        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(this.Frame, scissorOriginal);

        try
        {
            this.DrawInFrame(spriteBatch, cursor);
        }
        finally
        {
            spriteBatch.End();

            spriteBatch.Begin(
                sortModeOriginal,
                blendStateOriginal,
                samplerStateOriginal,
                depthStencilStateOriginal,
                rasterizerStateOriginal,
                effectOriginal);

            spriteBatch.GraphicsDevice.ScissorRectangle = scissorOriginal;
        }

        if (this.scrollBar.visible)
        {
            this.scrollBar.Draw(spriteBatch, cursor, Point.Zero);
        }
    }

    /// <inheritdoc />
    public abstract void DrawInFrame(SpriteBatch spriteBatch, Point cursor);

    /// <inheritdoc />
    public override ICustomMenu MoveTo(Point position)
    {
        base.MoveTo(position);
        this.scrollBar.MoveTo(new Point(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + 4));
        return this;
    }

    /// <inheritdoc />
    public override ICustomMenu ResizeTo(Point size)
    {
        base.ResizeTo(size);
        this.scrollBar.ResizeTo(new Point(0, size.Y)).MoveTo(new Point(this.Bounds.Right + 4, this.Bounds.Top + 4));
        return this;
    }

    /// <inheritdoc />
    public IFramedMenu SetCurrentOffset(Point value)
    {
        this.offset.X = Math.Clamp(value.X, 0, this.maxOffset.X);
        this.offset.Y = Math.Clamp(value.Y, 0, this.maxOffset.Y);
        return this;
    }

    /// <inheritdoc />
    public IFramedMenu SetMaxOffset(Point value)
    {
        this.maxOffset.X = Math.Max(-1, value.X);
        this.maxOffset.Y = Math.Max(-1, value.Y);
        this.scrollBar.visible = this.maxOffset.Y > -1;
        return this;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor) => this.scrollBar.visible && this.scrollBar.TryLeftClick(cursor);

    /// <inheritdoc />
    public override bool TryScroll(int direction) => this.scrollBar.visible && this.scrollBar.TryScroll(direction);

    /// <inheritdoc />
    public override void Update(Point cursor) => this.scrollBar.Update(cursor);
}