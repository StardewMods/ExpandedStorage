namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class ManagedTexture : IManagedTexture
{
    private readonly IRawTextureData data;
    private readonly IGameContentHelper gameContentHelper;

    private Texture2D? cachedTexture;

    /// <summary>Initializes a new instance of the <see cref="ManagedTexture" /> class.</summary>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="path">The game path to the texture.</param>
    /// <param name="data">The raw data for the source texture.</param>
    public ManagedTexture(IGameContentHelper gameContentHelper, string path, IRawTextureData data)
    {
        this.data = data;
        this.gameContentHelper = gameContentHelper;
        this.Name = gameContentHelper.ParseAssetName(path);
        this.RawData = [..data.Data];
    }

    /// <inheritdoc />
    public Color[] Data => this.data.Data;

    /// <inheritdoc />
    public int Height => this.data.Height;

    /// <inheritdoc />
    public IAssetName Name { get; }

    /// <inheritdoc />
    public Color[] RawData { get; }

    /// <inheritdoc />
    public Texture2D Value => this.cachedTexture ??= this.gameContentHelper.Load<Texture2D>(this.Name);

    /// <inheritdoc />
    public int Width => this.data.Width;

    /// <summary>Invalidates the texture cache.</summary>
    public void InvalidateCache() => this.cachedTexture = null;
}