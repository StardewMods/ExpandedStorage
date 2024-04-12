namespace StardewMods.FauxCore.Framework.Models;

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
        this.gameContentHelper = gameContentHelper;
        this.RawData = data;
        this.Name = gameContentHelper.ParseAssetName(path);
    }

    /// <summary>Gets the raw data for the base texture.</summary>
    public IRawTextureData RawData { get; }

    /// <inheritdoc />
    public IAssetName Name { get; }

    /// <inheritdoc />
    public Texture2D Value => this.cachedTexture ??= this.gameContentHelper.Load<Texture2D>(this.Name);

    /// <summary>Invalidates the texture cache.</summary>
    public void InvalidateCache() => this.cachedTexture = null;
}