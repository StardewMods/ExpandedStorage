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
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ThemeHelper(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        eventSubscriber.Subscribe<AssetReadyEventArgs>(this.OnAssetReady);
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventSubscriber.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventSubscriber.Subscribe<SaveLoadedEventArgs>(this.OnSaveLoaded);
    }

    /// <inheritdoc />
    public IManagedTexture AddAsset(string path, IRawTextureData data)
    {
        var managedTexture = new ManagedTexture(this.gameContentHelper, path, data);
        if (!this.trackedAssets.TryAdd(managedTexture.Name, managedTexture))
        {
            this.Log.Trace("Error, conflicting key {0} found in ThemeHelper. Asset not added.", managedTexture.Name);
        }

        return managedTexture;
    }

    private void InitializePalette()
    {
        var changed = false;
        var colors = new Color[Game1.mouseCursors.Width * Game1.mouseCursors.Height];
        Game1.mouseCursors.GetData(colors);
        foreach (var (points, color) in this.vanillaPalette)
        {
            var sample = points
                .Select(point => colors[point.X + (point.Y * Game1.mouseCursors.Width)])
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
                var rawTexture = managedTexture.RawData;
                if (this.paletteSwap.Any())
                {
                    for (var index = 0; index < rawTexture.Data.Length; ++index)
                    {
                        if (this.paletteSwap.TryGetValue(rawTexture.Data[index], out var newColor))
                        {
                            rawTexture.Data[index] = newColor;
                        }
                    }
                }

                var texture = new Texture2D(Game1.spriteBatch.GraphicsDevice, rawTexture.Width, rawTexture.Height);
                texture.SetData(rawTexture.Data);
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