namespace StardewMods.FauxCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.FauxCore.Framework.Interfaces.IAssetHandlerExtension" />
internal sealed class AssetHandler : BaseAssetHandler, IAssetHandlerExtension, IThemeHelper
{
    private static readonly Dictionary<VanillaIcon, Rectangle> VanillaIcons = new()
    {
        { VanillaIcon.ArrowDown, new Rectangle(421, 472, 11, 12) },
        { VanillaIcon.ArrowLeft, new Rectangle(352, 495, 12, 11) },
        { VanillaIcon.ArrowRight, new Rectangle(365, 495, 12, 11) },
        { VanillaIcon.ArrowUp, new Rectangle(421, 459, 11, 12) },
        { VanillaIcon.Backpack, new Rectangle(4, 372, 8, 11) },
        { VanillaIcon.Cancel, new Rectangle(192, 256, 64, 64) },
        { VanillaIcon.Checked, OptionsCheckbox.sourceRectChecked },
        { VanillaIcon.Chest, new Rectangle(127, 412, 10, 11) },
        { VanillaIcon.Coin, new Rectangle(4, 388, 8, 8) },
        { VanillaIcon.DoNot, new Rectangle(322, 498, 12, 12) },
        { VanillaIcon.EmptyHeart, new Rectangle(218, 428, 7, 6) },
        { VanillaIcon.Fish, new Rectangle(20, 428, 10, 10) },
        { VanillaIcon.FishingChest, new Rectangle(137, 412, 10, 11) },
        { VanillaIcon.Gift, new Rectangle(147, 412, 10, 11) },
        { VanillaIcon.Heart, new Rectangle(211, 428, 7, 6) },
        { VanillaIcon.Ok, new Rectangle(128, 256, 64, 64) },
        { VanillaIcon.Organize, new Rectangle(162, 440, 16, 16) },
        { VanillaIcon.QualityGold, new Rectangle(346, 400, 8, 8) },
        { VanillaIcon.QualityIridium, new Rectangle(346, 392, 8, 8) },
        { VanillaIcon.QualitySilver, new Rectangle(338, 400, 8, 8) },
        { VanillaIcon.Shield, new Rectangle(110, 428, 10, 10) },
        { VanillaIcon.Skull, new Rectangle(140, 428, 10, 10) },
        { VanillaIcon.Sword, new Rectangle(120, 428, 10, 10) },
        { VanillaIcon.Tool, new Rectangle(30, 428, 10, 10) },
        { VanillaIcon.Trash, new Rectangle(323, 433, 9, 10) },
        { VanillaIcon.Unchecked, OptionsCheckbox.sourceRectUnchecked },
        { VanillaIcon.Vegetable, new Rectangle(10, 428, 10, 10) },
    };

    private static readonly Dictionary<Point[], Color> VanillaPalette = new()
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

    private readonly Dictionary<IAssetName, IRawTextureData> cachedRawTextures = new();
    private readonly IconRegistry iconRegistry;
    private readonly Dictionary<Color, Color> paletteSwap = new();

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    public AssetHandler(
        ContentPatcherIntegration contentPatcherIntegration,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModContentHelper modContentHelper)
        : base(contentPatcherIntegration, eventManager, gameContentHelper, modContentHelper)
    {
        this.iconRegistry = new IconRegistry(this, Mod.Manifest);

        this
            .Asset($"{Mod.Id}/Icons")
            .Load(static () => new Dictionary<string, IconData>(StringComparer.OrdinalIgnoreCase))
            .Watch(this.RefreshIcons, _ => this.RefreshIcons());

        this.Asset("LooseSprites/Cursors").Watch(this.RefreshPalette, _ => this.RefreshPalette());

        this.AddAsset($"{Mod.Id}/UI", modContentHelper.Load<IRawTextureData>("assets/ui.png"));

        foreach (var (key, area) in AssetHandler.VanillaIcons)
        {
            this.iconRegistry.Add(key.ToStringFast(), "LooseSprites/Cursors", area);
        }
    }

    /// <inheritdoc />
    public void AddAsset(string path, IRawTextureData data)
    {
        var assetName = this.GameContentHelper.ParseAssetName(path);
        if (this.cachedRawTextures.ContainsKey(assetName))
        {
            Log.Trace("Error, conflicting key {0} found in ThemeHelper. Asset not added.", assetName.Name);
            return;
        }

        Log.Trace("Adding asset to theme helper: {0}", path);
        this.cachedRawTextures.TryAdd(assetName, data);
        var asset = this.Asset(assetName);
        asset.Load(() => this.LoadAsset(assetName));
        this.Asset("LooseSprites/Cursors").Watch(onInvalidated: _ => asset.Invalidate());
    }

    /// <inheritdoc />
    public void AddAsset(IIcon icon)
    {
        var assetName = this.GameContentHelper.ParseAssetName($"{Mod.Id}/Buttons/{icon.Id}");
        var asset = this.Asset(assetName);
        asset.Load(() => this.LoadButtonTexture(icon));
        this.Asset(icon.Path).Watch(onInvalidated: _ => asset.Invalidate());
        this.Asset($"{Mod.Id}/UI").Watch(onInvalidated: _ => asset.Invalidate());
    }

    private object LoadAsset(IAssetName assetName)
    {
        if (!this.cachedRawTextures.TryGetValue(assetName, out var rawTexture))
        {
            throw new KeyNotFoundException($"No asset data found for key: {assetName}");
        }

        Log.Trace("Generating texture for asset {0}", assetName.Name);
        var texture = new Texture2D(Game1.spriteBatch.GraphicsDevice, rawTexture.Width, rawTexture.Height);
        texture.SetData(rawTexture.Data.Select(color => this.paletteSwap.GetValueOrDefault(color, color)).ToArray());
        return texture;
    }

    private Texture2D LoadButtonTexture(IIcon icon)
    {
        Log.Trace("Generating button texture for icon {0}", icon.Id);

        // Generate button texture
        var length = (int)(16 * Math.Ceiling(icon.Area.Width / 16d));
        var scale = length / 16;
        var area = length * length;
        var colors = new Color[area];

        // Get ui texture
        var uiAsset = this.GameContentHelper.ParseAssetName($"{Mod.Id}/UI");
        if (!this.cachedRawTextures.TryGetValue(uiAsset, out var uiRawTexture))
        {
            var uiTexture = this.Asset(uiAsset).Require<Texture2D>();
            uiRawTexture = new VanillaTexture(uiTexture);
        }

        // Copy base to colors
        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < length; y++)
            {
                var targetIndex = (y * length) + x;
                var sourceX = x / scale;
                var sourceY = y / scale;
                var sourceIndex = (sourceY * 16) + sourceX;
                var color = uiRawTexture.Data[sourceIndex];
                colors[targetIndex] = this.paletteSwap.GetValueOrDefault(color, color);
            }
        }

        // Get icon texture
        var iconAsset = this.GameContentHelper.ParseAssetName(icon.Path);
        Color[]? iconData = null;
        if (!this.cachedRawTextures.TryGetValue(iconAsset, out var iconRawTexture))
        {
            var iconTexture = this.Asset(iconAsset).Require<Texture2D>();
            iconRawTexture = new VanillaTexture(iconTexture);
            iconData = iconRawTexture.Data;
        }

        iconData ??= iconRawTexture.Data.Select(color => this.paletteSwap.GetValueOrDefault(color, color)).ToArray();

        // Copy icon to colors
        var xOffset = (length - icon.Area.Width) / 2;
        var yOffset = (length - icon.Area.Height) / 2;
        for (var x = xOffset; x < xOffset + icon.Area.Width; x++)
        {
            for (var y = yOffset; y < yOffset + icon.Area.Height; y++)
            {
                var targetIndex = (y * length) + x;
                var sourceX = x - xOffset + icon.Area.X;
                var sourceY = y - yOffset + icon.Area.Y;
                var sourceIndex = (sourceY * iconRawTexture.Width) + sourceX;
                var color = iconData[sourceIndex];
                if (color.A > 0)
                {
                    colors[targetIndex] = color;
                }
            }
        }

        // Create texture
        var texture = new Texture2D(Game1.spriteBatch.GraphicsDevice, length, length);
        texture.SetData(colors);
        return texture;
    }

    private void RefreshIcons()
    {
        foreach (var (key, icon) in this.Asset($"{Mod.Id}/Icons").Require<Dictionary<string, IconData>>())
        {
            this.iconRegistry.Add(key, icon.Path, icon.Area);
        }
    }

    private void RefreshPalette()
    {
        var mouseCursors = new VanillaTexture(Game1.mouseCursors);
        this.paletteSwap.Clear();
        foreach (var (points, color) in AssetHandler.VanillaPalette)
        {
            this.paletteSwap[color] = points
                .Select(point => mouseCursors.Data[point.X + (point.Y * mouseCursors.Width)])
                .GroupBy(sample => sample)
                .OrderByDescending(group => group.Count())
                .First()
                .Key;
        }
    }
}