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

/// <summary>Base custom component.</summary>
internal abstract class BaseComponent : ClickableComponent, ICustomComponent
{
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="BaseComponent" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="name">The component name.</param>
    protected BaseComponent(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        int x,
        int y,
        int width,
        int height,
        string name)
        : base(new Rectangle(x, y, width, height), name)
    {
        this.Input = inputHelper;
        this.reflectionHelper = reflectionHelper;
    }

    /// <inheritdoc />
    public Point Position => this.bounds.Location;

    /// <inheritdoc />
    public string? HoverText { get; private set; }

    /// <summary>Gets the input helper.</summary>
    protected IInputHelper Input { get; }

    /// <inheritdoc />
    public virtual void Draw(SpriteBatch spriteBatch, Point cursor, Point offset)
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

        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(
            new Rectangle(this.bounds.X + offset.X, this.bounds.Y + offset.Y, this.bounds.Width, this.bounds.Height),
            scissorOriginal);

        try
        {
            this.DrawInFrame(spriteBatch, cursor, offset);
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
    }

    /// <inheritdoc />
    public virtual void DrawInFrame(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        // Do nothing
    }

    /// <inheritdoc />
    public virtual ICustomComponent MoveTo(Point location)
    {
        this.bounds.Location = location;
        return this;
    }

    /// <inheritdoc />
    public virtual ICustomComponent ResizeTo(Point size)
    {
        this.bounds.Size = size;
        return this;
    }

    /// <inheritdoc />
    public ICustomComponent SetHoverText(string? value)
    {
        this.HoverText = value;
        return this;
    }

    /// <inheritdoc />
    public virtual bool TryLeftClick(Point cursor) => this.bounds.Contains(cursor);

    /// <inheritdoc />
    public virtual bool TryRightClick(Point cursor) => this.bounds.Contains(cursor);

    /// <inheritdoc />
    public virtual bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public virtual void Update(Point cursor) { }
}