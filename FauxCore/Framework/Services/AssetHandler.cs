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

/// <inheritdoc cref="StardewMods.FauxCore.Framework.Interfaces.IAssetHandler" />
internal sealed class AssetHandler : BaseService, IAssetHandler
{
    private readonly Dictionary<string, IManagedTexture> cachedData = new();
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

        // Get icon base
        if (!this.themeHelper.TryGetAsset(this.ModId + "/UI", out var baseTexture))
        {
            throw new InvalidOperationException("The ui texture is missing.");
        }

        var colors = new Color[16 * 16];

        // Copy base to colors
        for (var i = 0; i < colors.Length; i++)
        {
            var x = i % 16;
            var y = i / 16;
            colors[i] = baseTexture.Data[(y * baseTexture.Width) + x];
        }

        // Get icon data
        if (!this.themeHelper.TryGetAsset(icon.Path, out var managedTexture)
            && !this.cachedData.TryGetValue(icon.Path, out managedTexture))
        {
            var iconData = new VanillaTexture(icon.GetTexture(IconStyle.Transparent));
            managedTexture = new ManagedTexture(this.gameContentHelper, icon.Path, iconData);
            this.cachedData.Add(icon.Id, managedTexture);
        }

        // Copy icon to colors
        var xOffset = (16 - icon.Area.Width) / 2;
        var yOffset = (16 - icon.Area.Height) / 2;
        for (var i = 0; i < icon.Area.Width * icon.Area.Height; i++)
        {
            var x = i % icon.Area.Width;
            var y = i / icon.Area.Width;
            var j = x + xOffset + ((y + yOffset) * 16);
            var k = x + icon.Area.Left + ((y + icon.Area.Top) * managedTexture.Width);
            if (managedTexture.Data[k].A > 0)
            {
                colors[j] = managedTexture.Data[k];
            }
        }

        // Create texture
        texture = new Texture2D(Game1.spriteBatch.GraphicsDevice, 16, 16);
        texture.SetData(colors);
        this.cachedTextures.TryAdd($"{this.ModId}/Buttons/{icon.Id}", texture);
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
        this.iconRegistry.Add("SDV.Vanilla/Backpack", "LooseSprites/Cursors", new Rectangle(4, 372, 8, 11));
        this.iconRegistry.Add("SDV.Vanilla/Shipping", "LooseSprites/Cursors", new Rectangle(4, 388, 8, 8));
        this.iconRegistry.Add("SDV.Vanilla/UpArrow", "LooseSprites/Cursors", new Rectangle(421, 459, 11, 12));
        this.iconRegistry.Add("SDV.Vanilla/DownArrow", "LooseSprites/Cursors", new Rectangle(421, 472, 11, 12));
        this.iconRegistry.Add("SDV.Vanilla/LeftArrow", "LooseSprites/Cursors", new Rectangle(352, 495, 12, 11));
        this.iconRegistry.Add("SDV.Vanilla/RightArrow", "LooseSprites/Cursors", new Rectangle(365, 495, 12, 11));
        this.iconRegistry.Add("SDV.Vanilla/Chest", "LooseSprites/Cursors", new Rectangle(127, 412, 10, 11));
        this.iconRegistry.Add("SDV.Vanilla/Chest", "LooseSprites/Cursors", new Rectangle(127, 412, 10, 11));
        this.iconRegistry.Add("SDV.Vanilla/FishingChest", "LooseSprites/Cursors", new Rectangle(137, 412, 10, 11));
        this.iconRegistry.Add("SDV.Vanilla/Gift", "LooseSprites/Cursors", new Rectangle(147, 412, 10, 11));
        this.iconRegistry.Add("SDV.Vanilla/Skull", "LooseSprites/Cursors", new Rectangle(140, 428, 10, 10));
        this.iconRegistry.Add("SDV.Vanilla/Sword", "LooseSprites/Cursors", new Rectangle(120, 428, 10, 10));
        this.iconRegistry.Add("SDV.Vanilla/Shield", "LooseSprites/Cursors", new Rectangle(110, 428, 10, 10));
        this.iconRegistry.Add("SDV.Vanilla/Tool", "LooseSprites/Cursors", new Rectangle(30, 428, 10, 10));
        this.iconRegistry.Add("SDV.Vanilla/Fishing", "LooseSprites/Cursors", new Rectangle(20, 428, 10, 10));
        this.iconRegistry.Add("SDV.Vanilla/Crops", "LooseSprites/Cursors", new Rectangle(10, 428, 10, 10));
        this.iconRegistry.Add("SDV.Vanilla/Trash", "LooseSprites/Cursors", new Rectangle(323, 433, 9, 10));
        this.iconRegistry.Add("SDV.Vanilla/DoNot", "LooseSprites/Cursors", new Rectangle(322, 498, 12, 12));
        this.iconRegistry.Add("SDV.Vanilla/Silver", "LooseSprites/Cursors", new Rectangle(338, 400, 8, 8));
        this.iconRegistry.Add("SDV.Vanilla/Gold", "LooseSprites/Cursors", new Rectangle(346, 400, 8, 8));
        this.iconRegistry.Add("SDV.Vanilla/Iridium", "LooseSprites/Cursors", new Rectangle(346, 392, 8, 8));
    }

    private void RefreshIcons()
    {
        var icons = this.gameContentHelper.Load<Dictionary<string, IconData>>($"{this.ModId}/Icons");
        foreach (var (key, icon) in icons)
        {
            this.iconRegistry.AddIcon(key, icon.Path, icon.Area);
        }
    }
}