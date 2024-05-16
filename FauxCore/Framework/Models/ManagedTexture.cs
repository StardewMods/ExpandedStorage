namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class ManagedTexture : IManagedTexture
{
    private readonly IGameContentHelper gameContentHelper;

    private Texture2D? cachedTexture;

    /// <summary>Initializes a new instance of the <see cref="ManagedTexture" /> class.</summary>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="path">The game path to the texture.</param>
    /// <param name="data">The raw data for the source texture.</param>
    public ManagedTexture(IGameContentHelper gameContentHelper, string path, IRawTextureData data)
    {
        this.RawData = data.Data;
        this.Data = new Color[data.Data.Length];
        Array.Copy(data.Data, this.Data, data.Data.Length);
        this.Height = data.Height;
        this.Width = data.Width;
        this.gameContentHelper = gameContentHelper;
        this.Name = gameContentHelper.ParseAssetName(path);
    }

    /// <inheritdoc />
    public Color[] Data { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public IAssetName Name { get; }

    /// <summary>Gets the raw data of the texture.</summary>
    public Color[] RawData { get; }

    /// <inheritdoc />
    public Texture2D Value => this.cachedTexture ??= this.gameContentHelper.Load<Texture2D>(this.Name);

    /// <inheritdoc />
    public int Width { get; }

    /// <summary>Invalidates the texture cache.</summary>
    public void InvalidateCache() => this.cachedTexture = null;
}