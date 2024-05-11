namespace StardewMods.Common.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Represents a ui manager service.</summary>
internal sealed class UiManager : BaseService
{
    private readonly IReflectionHelper reflectionHelper;

    public UiManager(ILog log, IManifest manifest, IReflectionHelper reflectionHelper)
        : base(log, manifest) =>
        this.reflectionHelper = reflectionHelper;

    public void DrawBox(
        SpriteBatch b,
        Texture2D texture,
        Rectangle sourceRect,
        int x,
        int y,
        int width,
        int height,
        Color color,
        float scale = 1f,
        bool drawShadow = true,
        float drawLayer = -1f)
    {
        var num = sourceRect.Width / 3;
        var layerDepth = drawLayer - 0.03f;
        if (drawLayer < 0.0)
        {
            drawLayer = (float)(0.800000011920929 - (y * 9.999999974752427E-07));
            layerDepth = 0.77f;
        }

        if (drawShadow)
        {
            b.Draw(
                texture,
                new Vector2(x + width - (int)(num * (double)scale) - 8, y + 8),
                new Rectangle(sourceRect.X + (num * 2), sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Vector2(x - 8, y + height - (int)(num * (double)scale) + 8),
                new Rectangle(sourceRect.X, (num * 2) + sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Vector2(x + width - (int)(num * (double)scale) - 8, y + height - (int)(num * (double)scale) + 8),
                new Rectangle(sourceRect.X + (num * 2), (num * 2) + sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Rectangle(
                    x + (int)(num * (double)scale) - 8,
                    y + 8,
                    width - ((int)(num * (double)scale) * 2),
                    (int)(num * (double)scale)),
                new Rectangle(sourceRect.X + num, sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Rectangle(
                    x + (int)(num * (double)scale) - 8,
                    y + height - (int)(num * (double)scale) + 8,
                    width - ((int)(num * (double)scale) * 2),
                    (int)(num * (double)scale)),
                new Rectangle(sourceRect.X + num, (num * 2) + sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Rectangle(
                    x - 8,
                    y + (int)(num * (double)scale) + 8,
                    (int)(num * (double)scale),
                    height - ((int)(num * (double)scale) * 2)),
                new Rectangle(sourceRect.X, num + sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Rectangle(
                    x + width - (int)(num * (double)scale) - 8,
                    y + (int)(num * (double)scale) + 8,
                    (int)(num * (double)scale),
                    height - ((int)(num * (double)scale) * 2)),
                new Rectangle(sourceRect.X + (num * 2), num + sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);

            b.Draw(
                texture,
                new Rectangle(
                    (int)(num * (double)scale / 2.0) + x - 8,
                    (int)(num * (double)scale / 2.0) + y + 8,
                    width - (int)(num * (double)scale),
                    height - (int)(num * (double)scale)),
                new Rectangle(num + sourceRect.X, num + sourceRect.Y, num, num),
                Color.Black * 0.4f,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);
        }

        b.Draw(
            texture,
            new Rectangle(
                (int)(num * (double)scale) + x,
                (int)(num * (double)scale) + y,
                width - (int)(num * (double)scale * 2.0),
                height - (int)(num * (double)scale * 2.0)),
            new Rectangle(num + sourceRect.X, num + sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Vector2(x, y),
            new Rectangle(sourceRect.X, sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Vector2(x + width - (int)(num * (double)scale), y),
            new Rectangle(sourceRect.X + (num * 2), sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Vector2(x, y + height - (int)(num * (double)scale)),
            new Rectangle(sourceRect.X, (num * 2) + sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Vector2(x + width - (int)(num * (double)scale), y + height - (int)(num * (double)scale)),
            new Rectangle(sourceRect.X + (num * 2), (num * 2) + sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Rectangle(
                x + (int)(num * (double)scale),
                y,
                width - ((int)(num * (double)scale) * 2),
                (int)(num * (double)scale)),
            new Rectangle(sourceRect.X + num, sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Rectangle(
                x + (int)(num * (double)scale),
                y + height - (int)(num * (double)scale),
                width - ((int)(num * (double)scale) * 2),
                (int)(num * (double)scale)),
            new Rectangle(sourceRect.X + num, (num * 2) + sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Rectangle(
                x,
                y + (int)(num * (double)scale),
                (int)(num * (double)scale),
                height - ((int)(num * (double)scale) * 2)),
            new Rectangle(sourceRect.X, num + sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            SpriteEffects.None,
            drawLayer);

        b.Draw(
            texture,
            new Rectangle(
                x + width - (int)(num * (double)scale),
                y + (int)(num * (double)scale),
                (int)(num * (double)scale),
                height - ((int)(num * (double)scale) * 2)),
            new Rectangle(sourceRect.X + (num * 2), num + sourceRect.Y, num, num),
            color,
            0.0f,
            Vector2.Zero,
            SpriteEffects.None,
            drawLayer);
    }

    /// <summary>Draws the specified render action within a specified area of the screen.</summary>
    /// <param name="b">The SpriteBatch used for drawing.</param>
    /// <param name="mode">The SpriteSortMode used for drawing.</param>
    /// <param name="area">The Rectangle specifying the area to draw within.</param>
    /// <param name="render">The action containing the rendering code.</param>
    public void DrawInFrame(SpriteBatch b, SpriteSortMode mode, Rectangle area, Action render)
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

        b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(area, scissorOriginal);

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