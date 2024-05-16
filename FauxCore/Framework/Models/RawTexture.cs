namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;

/// <inheritdoc />
internal class RawTexture : IRawTextureData
{
    /// <summary>Initializes a new instance of the <see cref="RawTexture" /> class.</summary>
    /// <param name="width">The texture width.</param>
    /// <param name="height">The texture height.</param>
    /// <param name="data">The texture data.</param>
    public RawTexture(int width, int height, Color[] data)
    {
        this.Width = width;
        this.Height = height;
        this.Data = data;
    }

    /// <inheritdoc />
    public Color[] Data { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public int Width { get; }
}