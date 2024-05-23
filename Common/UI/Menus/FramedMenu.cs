#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.UI.Components;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.UI.Components;
#endif

/// <summary>Framed menu with vertical scrolling.</summary>
internal abstract class FramedMenu : BaseMenu
{
    private readonly IReflectionHelper reflectionHelper;

    private int maxOffset;
    private int offset;

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
        this.ScrollBar = new VerticalScrollBar(
            this.Input,
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + 4,
            this.height,
            () => this.Offset,
            value =>
            {
                this.Offset = value;
            },
            () => 0,
            () => this.MaxOffset,
            () => this.StepSize);

        this.allClickableComponents.Add(this.ScrollBar);
    }

    /// <summary>Gets or sets the maximum offset.</summary>
    public int MaxOffset
    {
        get => this.maxOffset;
        protected set
        {
            this.maxOffset = value;
            this.ScrollBar.visible = this.maxOffset > -1;
        }
    }

    /// <summary>Gets or sets the y-offset.</summary>
    public int Offset
    {
        get => this.offset;
        set => this.offset = Math.Min(this.MaxOffset, Math.Max(0, value));
    }

    /// <summary>Gets the frame.</summary>
    protected virtual Rectangle Frame { get; } = Rectangle.Empty;

    /// <summary>Gets the scrollbar.</summary>
    protected VerticalScrollBar ScrollBar { get; }

    /// <summary>Gets the step size for scrolling.</summary>
    protected virtual int StepSize => 1;

    /// <inheritdoc />
    public override void MoveTo(int x, int y)
    {
        base.MoveTo(x, y);
        this.ScrollBar.MoveTo(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + 4);
    }

    /// <inheritdoc />
    public override void Resize(int newWidth, int newHeight)
    {
        base.Resize(newWidth, newHeight);
        this.ScrollBar.Resize(0, this.height);
        this.ScrollBar.MoveTo(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + 4);
    }

    /// <summary>Draws the specified render action within a specified area of the screen.</summary>
    /// <param name="b">The SpriteBatch used for drawing.</param>
    /// <param name="mode">The SpriteSortMode used for drawing.</param>
    /// <param name="render">The action containing the rendering code.</param>
    protected void DrawInFrame(SpriteBatch b, SpriteSortMode mode, Action render)
    {
        var sortModeReflected = this.reflectionHelper.GetField<SpriteSortMode>(b, "_sortMode", false);
        var sortModeOriginal = sortModeReflected?.GetValue() ?? mode;

        var blendStateReflected = this.reflectionHelper.GetField<BlendState>(b, "_blendState", false);
        var blendStateOriginal = blendStateReflected?.GetValue();

        var samplerStateReflected = this.reflectionHelper.GetField<SamplerState>(b, "_samplerState", false);
        var samplerStateOriginal = samplerStateReflected?.GetValue();

        var depthStencilStateReflected =
            this.reflectionHelper.GetField<DepthStencilState>(b, "_depthStencilState", false);

        var depthStencilStateOriginal = depthStencilStateReflected?.GetValue();

        var rasterizerStateReflected = this.reflectionHelper.GetField<RasterizerState>(b, "_rasterizerState", false);
        var rasterizerStateOriginal = rasterizerStateReflected?.GetValue();

        var effectReflected = this.reflectionHelper.GetField<Effect>(b, "_effect", false);
        var effectOriginal = effectReflected?.GetValue();

        var scissorOriginal = b.GraphicsDevice.ScissorRectangle;

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

        b.End();

        b.Begin(
            mode,
            blendStateOriginal,
            samplerStateOriginal,
            depthStencilStateOriginal,
            rasterizerState,
            effectOriginal);

        b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(this.Frame, scissorOriginal);

        try
        {
            render.Invoke();
        }
        finally
        {
            b.End();

            b.Begin(
                sortModeOriginal,
                blendStateOriginal,
                samplerStateOriginal,
                depthStencilStateOriginal,
                rasterizerStateOriginal,
                effectOriginal);

            b.GraphicsDevice.ScissorRectangle = scissorOriginal;
        }
    }
}