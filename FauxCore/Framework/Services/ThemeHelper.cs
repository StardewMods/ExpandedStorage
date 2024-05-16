namespace StardewMods.FauxCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Models;

/// <inheritdoc cref="IThemeHelper" />
internal sealed class ThemeHelper : BaseService, IThemeHelper
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly Dictionary<Color, Color> paletteSwap = new();
    private readonly Dictionary<IAssetName, ManagedTexture> trackedAssets = [];

    private readonly Dictionary<Point[], Color> vanillaPalette = new()
    {
        // Outside edge of frame
        { [new Point(17, 369), new Point(104, 469), new Point(118, 483)], new Color(91, 43, 42) },

        // Inner frame color
        { [new Point(18, 370), new Point(105, 471), new Point(116, 483)], new Color(220, 123, 5) },

        // Dark shade of inner frame
        { [new Point(19, 371), new Point(106, 475), new Point(115, 475)], new Color(177, 78, 5) },

        // Dark shade of menu background
        { [new Point(20, 372), new Point(28, 378), new Point(22, 383)], new Color(228, 174, 110) },

        // Menu background
        { [new Point(21, 373), new Point(26, 377), new Point(21, 381)], new Color(255, 210, 132) },

        // Highlight of menu button
        { [new Point(104, 471), new Point(111, 470), new Point(117, 480)], new Color(247, 186, 0) },
    };

    private bool initialize;

    /// <summary>Initializes a new instance of the <see cref="ThemeHelper" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ThemeHelper(IEventManager eventManager, IGameContentHelper gameContentHelper, ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        eventManager.Subscribe<AssetReadyEventArgs>(this.OnAssetReady);
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<SaveLoadedEventArgs>(this.OnSaveLoaded);
    }

    /// <inheritdoc />
    public void AddAsset(string path, IRawTextureData data)
    {
        var assetName = this.gameContentHelper.ParseAssetName(path);
        if (this.trackedAssets.ContainsKey(assetName))
        {
            this.Log.Trace("Error, conflicting key {0} found in ThemeHelper. Asset not added.", assetName.Name);
        }

        var managedTexture = new ManagedTexture(this.gameContentHelper, path, data);
        this.trackedAssets.TryAdd(assetName, managedTexture);
    }

    /// <inheritdoc />
    public IManagedTexture RequireAsset(string path)
    {
        if (this.TryGetAsset(path, out var texture))
        {
            return texture;
        }

        throw new KeyNotFoundException($"No asset found for path: {path}");
    }

    /// <inheritdoc />
    public bool TryGetAsset(string path, [NotNullWhen(true)] out IManagedTexture? texture)
    {
        var assetName = this.gameContentHelper.ParseAssetName(path);
        if (!this.trackedAssets.TryGetValue(assetName, out var managedTexture))
        {
            texture = null;
            return false;
        }

        texture = managedTexture;
        return true;
    }

    private void InitializePalette()
    {
        var changed = false;
        var mouseCursors = new VanillaTexture(Game1.mouseCursors);
        foreach (var (points, color) in this.vanillaPalette)
        {
            var sample = points
                .Select(point => mouseCursors.Data[point.X + (point.Y * mouseCursors.Width)])
                .GroupBy(sample => sample)
                .OrderByDescending(group => group.Count())
                .First()
                .Key;

            if (color.Equals(sample)
                || (this.paletteSwap.TryGetValue(color, out var currentColor) && currentColor == sample))
            {
                continue;
            }

            this.paletteSwap[color] = sample;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        foreach (var (assetName, managedTexture) in this.trackedAssets)
        {
            managedTexture.InvalidateCache();
            this.gameContentHelper.InvalidateCache(assetName);

            for (var index = 0; index < managedTexture.Data.Length; ++index)
            {
                managedTexture.Data[index] =
                    this.paletteSwap.TryGetValue(managedTexture.RawData[index], out var newColor)
                        ? newColor
                        : managedTexture.RawData[index];
            }
        }
    }

    private void OnAssetReady(AssetReadyEventArgs e)
    {
        if (!this.initialize || !e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
        {
            return;
        }

        this.initialize = false;
        this.InitializePalette();
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (!this.trackedAssets.TryGetValue(e.NameWithoutLocale, out var managedTexture))
        {
            return;
        }

        e.LoadFrom(
            () =>
            {
                var texture = new Texture2D(
                    Game1.spriteBatch.GraphicsDevice,
                    managedTexture.Width,
                    managedTexture.Height);

                texture.SetData(managedTexture.Data);
                return texture;
            },
            AssetLoadPriority.Medium);
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo("LooseSprites/Cursors")))
        {
            this.initialize = true;
        }
    }

    private void OnSaveLoaded(SaveLoadedEventArgs e) => this.InitializePalette();
}