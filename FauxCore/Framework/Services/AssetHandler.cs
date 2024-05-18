namespace StardewMods.FauxCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.FauxCore.Framework.Interfaces.IAssetHandler" />
internal sealed class AssetHandler : BaseService, IAssetHandler
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

    private readonly Dictionary<string, Texture2D> cachedTextures = new();
    private readonly IGameContentHelper gameContentHelper;
    private readonly IconRegistry iconRegistry;
    private readonly ThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IModContentHelper modContentHelper,
        ThemeHelper themeHelper)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        this.themeHelper = themeHelper;
        this.iconRegistry = new IconRegistry(this, log, manifest);

        themeHelper.AddAsset(this.ModId + "/UI", modContentHelper.Load<IRawTextureData>("assets/ui.png"));

        // Events
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
    public Texture2D CreateButtonTexture(IIcon icon)
    {
        if (this.cachedTextures.TryGetValue($"{this.ModId}/Buttons/{icon.Id}", out var texture))
        {
            return texture;
        }

        // Get icon texture
        if (!this.themeHelper.TryGetRawTextureData(icon.Path, out var iconTexture))
        {
            throw new InvalidOperationException("The icon texture is missing.");
        }

        // Get button texture
        if (!this.themeHelper.TryGetRawTextureData(this.ModId + "/UI", out var baseTexture))
        {
            throw new InvalidOperationException("The ui texture is missing.");
        }

        var length = (int)(16 * Math.Ceiling(icon.Area.Width / 16d));
        var scale = length / 16;
        var area = length * length;
        var colors = new Color[area];

        // Copy base to colors
        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < length; y++)
            {
                var targetIndex = (y * length) + x;
                var sourceX = x / scale;
                var sourceY = y / scale;
                var sourceIndex = (sourceY * 16) + sourceX;
                colors[targetIndex] = baseTexture.Data[sourceIndex];
            }
        }

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
                var sourceIndex = (sourceY * iconTexture.Width) + sourceX;
                if (iconTexture.Data[sourceIndex].A > 0)
                {
                    colors[targetIndex] = iconTexture.Data[sourceIndex];
                }
            }
        }

        // Create texture
        texture = new Texture2D(Game1.spriteBatch.GraphicsDevice, length, length);
        texture.SetData(colors);
        this.cachedTextures.Add($"{this.ModId}/Buttons/{icon.Id}", texture);
        return texture;
    }

    /// <inheritdoc />
    public Texture2D GetTexture(IIcon icon)
    {
        if (this.cachedTextures.TryGetValue(icon.Path, out var texture))
        {
            return texture;
        }

        texture = this.gameContentHelper.Load<Texture2D>(icon.Path);
        this.cachedTextures.Add(icon.Path, texture);
        return texture;
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModId}/Icons"))
        {
            e.LoadFrom(() => new Dictionary<string, IconData>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo($"{this.ModId}/Icons")))
        {
            this.RefreshIcons();
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) => this.RefreshIcons();

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        foreach (var (key, area) in AssetHandler.VanillaIcons)
        {
            this.iconRegistry.Add(key.ToStringFast(), "LooseSprites/Cursors", area);
        }
    }

    private void RefreshIcons()
    {
        var icons = this.gameContentHelper.Load<Dictionary<string, IconData>>($"{this.ModId}/Icons");
        foreach (var (key, icon) in icons)
        {
            this.iconRegistry.Add(key, icon.Path, icon.Area);
        }
    }
}