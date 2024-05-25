#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#else
namespace StardewMods.Common.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

/// <summary>User interface helper service.</summary>
internal sealed class UiToolkit
{
    private static UiToolkit instance = null!;

    /// <summary>Initializes a new instance of the <see cref="UiToolkit" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public UiToolkit(IInputHelper inputHelper, IReflectionHelper reflectionHelper)
    {
        UiToolkit.instance = this;
        UiToolkit.Input = inputHelper;
        UiToolkit.Reflection = reflectionHelper;
    }

    /// <summary>Gets the cursor position.</summary>
    public static Point Cursor => UiToolkit.Input.GetCursorPosition().GetScaledScreenPixels().ToPoint();

    /// <summary>Gets the input helper.</summary>
    public static IInputHelper Input { get; private set; } = null!;

    /// <summary>Gets the reflection helper.</summary>
    public static IReflectionHelper Reflection { get; private set; } = null!;

    /// <summary>Draw to a framed sprite batch.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    /// <param name="frame">The framed area.</param>
    /// <param name="draw">The draw action.</param>
    public static void DrawInFrame(SpriteBatch spriteBatch, Rectangle frame, Action<SpriteBatch> draw)
    {
        var sortModeReflected = UiToolkit.Reflection.GetField<SpriteSortMode>(spriteBatch, "_sortMode", false);
        var sortModeOriginal = sortModeReflected?.GetValue() ?? SpriteSortMode.Deferred;

        var blendStateReflected = UiToolkit.Reflection.GetField<BlendState>(spriteBatch, "_blendState", false);
        var blendStateOriginal = blendStateReflected?.GetValue();

        var samplerStateReflected = UiToolkit.Reflection.GetField<SamplerState>(spriteBatch, "_samplerState", false);
        var samplerStateOriginal = samplerStateReflected?.GetValue();

        var depthStencilStateReflected =
            UiToolkit.Reflection.GetField<DepthStencilState>(spriteBatch, "_depthStencilState", false);

        var depthStencilStateOriginal = depthStencilStateReflected?.GetValue();

        var rasterizerStateReflected =
            UiToolkit.Reflection.GetField<RasterizerState>(spriteBatch, "_rasterizerState", false);

        var rasterizerStateOriginal = rasterizerStateReflected?.GetValue();

        var effectReflected = UiToolkit.Reflection.GetField<Effect>(spriteBatch, "_effect", false);
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

        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(frame, scissorOriginal);

        try
        {
            draw(spriteBatch);
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
}