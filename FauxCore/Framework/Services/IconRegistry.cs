namespace StardewMods.FauxCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.FauxCore.IIconRegistry" />
internal sealed class IconRegistry : IIconRegistry
{
    private static readonly Dictionary<string, Icon> Icons = new();

    private readonly IAssetHandlerExtension assetHandler;
    private readonly Dictionary<string, string> ids = [];
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="IconRegistry" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public IconRegistry(IAssetHandlerExtension assetHandler, IManifest manifest)
    {
        this.assetHandler = assetHandler;
        this.manifest = manifest;
    }

    /// <summary>Adds an icon to the icon registry.</summary>
    /// <param name="id">The unique identifier of the icon.</param>
    /// <param name="path">The icon texture path.</param>
    /// <param name="area">The icon source area.</param>
    /// <returns>The icon.</returns>
    public Icon Add(string id, string path, Rectangle area)
    {
        var icon = new Icon(this.GetTexture, this.CreateComponent)
        {
            Area = area,
            Id = id,
            Path = path,
        };

        if (IconRegistry.Icons.TryAdd(id, icon))
        {
            this.assetHandler.AddAsset(icon);
        }

        return icon;
    }

    /// <inheritdoc />
    public void AddIcon(string id, string path, Rectangle area)
    {
        var uniqueId = $"{this.manifest.UniqueID}/{id}";
        if (!IconRegistry.Icons.TryGetValue(uniqueId, out var icon))
        {
            icon = this.Add(uniqueId, path, area);
        }

        Log.Trace("Registering icon: {0}", uniqueId);
        this.ids.TryAdd(id, uniqueId);
        icon.Path = path;
        icon.Area = area;
    }

    /// <inheritdoc />
    public IEnumerable<IIcon> GetIcons() => IconRegistry.Icons.Values;

    /// <inheritdoc />
    public IIcon RequireIcon(string id)
    {
        if (this.TryGetIcon(id, out var icon))
        {
            return icon;
        }

        throw new KeyNotFoundException($"No icon found with the id: {id}.");
    }

    /// <inheritdoc />
    public IIcon RequireIcon(VanillaIcon icon) => this.RequireIcon(icon.ToStringFast());

    /// <inheritdoc />
    public bool TryGetIcon(string id, [NotNullWhen(true)] out IIcon? icon)
    {
        icon = null;
        if ((this.ids.TryGetValue(id, out var uniqueId) && IconRegistry.Icons.TryGetValue(uniqueId, out var value))
            || IconRegistry.Icons.TryGetValue(id, out value))
        {
            icon = value;
        }

        return icon is not null;
    }

    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Reviewed")]
    private ClickableTextureComponent CreateComponent(
        IIcon icon,
        IconStyle style,
        int x = 0,
        int y = 0,
        float scale = Game1.pixelZoom)
    {
        var texture = this.GetTexture(icon, style);
        scale = style switch
        {
            IconStyle.Transparent => Math.Min(
                4,
                (int)Math.Floor(16f * scale / Math.Max(icon.Area.Height, icon.Area.Width))),
            IconStyle.Button => 16f * scale / texture.Width,
            _ => scale,
        };

        return style switch
        {
            IconStyle.Transparent => new ClickableTextureComponent(
                icon.Id,
                new Rectangle(x, y, (int)(icon.Area.Width * scale), (int)(icon.Area.Height * scale)),
                null,
                null,
                texture,
                icon.Area,
                scale),
            IconStyle.Button => new ClickableTextureComponent(
                icon.Id,
                new Rectangle(x, y, (int)(scale * 16), (int)(scale * 16)),
                null,
                null,
                texture,
                new Rectangle(0, 0, texture.Width, texture.Height),
                scale),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }

    private Texture2D GetTexture(IIcon icon, IconStyle style) =>
        style switch
        {
            IconStyle.Transparent => this.assetHandler.Asset(icon.Path).Require<Texture2D>(),
            IconStyle.Button => this.assetHandler.Asset($"{Mod.Id}/Buttons/{icon.Id}").Require<Texture2D>(),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
}