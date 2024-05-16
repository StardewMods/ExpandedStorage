namespace StardewMods.FauxCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.FauxCore.IIconRegistry" />
internal sealed class IconRegistry : BaseService, IIconRegistry
{
    private static readonly Dictionary<string, Icon> Icons = new();

    private readonly IAssetHandler assetHandler;

    /// <summary>Initializes a new instance of the <see cref="IconRegistry" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public IconRegistry(IAssetHandler assetHandler, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.assetHandler = assetHandler;

    /// <summary>Adds an icon to the icon registry.</summary>
    /// <param name="id">The unique identifier of the icon.</param>
    /// <param name="path">The icon texture path.</param>
    /// <param name="area">The icon source area.</param>
    /// <returns>The icon.</returns>
    public Icon Add(string id, string path, Rectangle area)
    {
        var icon = new Icon(this.GetTexture, this.CreateComponent)
        {
            Id = id,
            Path = path,
            Area = area,
        };

        IconRegistry.Icons.TryAdd(id, icon);
        return icon;
    }

    /// <inheritdoc />
    public void AddIcon(string id, string path, Rectangle area)
    {
        if (!IconRegistry.Icons.TryGetValue($"{this.ModId}/{id}", out var icon))
        {
            icon = this.Add($"{this.ModId}/{id}", path, area);
        }

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
    public bool TryGetIcon(string id, [NotNullWhen(true)] out IIcon? icon)
    {
        icon = null;
        if (IconRegistry.Icons.TryGetValue($"{this.ModId}/{id}", out var value)
            || IconRegistry.Icons.TryGetValue(id, out value))
        {
            icon = value;
        }

        return icon is not null;
    }

    private ClickableTextureComponent CreateComponent(IIcon icon, ComponentStyle style) =>
        style switch
        {
            ComponentStyle.Transparent => new ClickableTextureComponent(
                icon.Id,
                new Rectangle(0, 0, icon.Area.Width * Game1.pixelZoom, icon.Area.Height * Game1.pixelZoom),
                null,
                null,
                this.assetHandler.GetTexture(icon),
                icon.Area,
                Game1.pixelZoom),
            ComponentStyle.Button => new ClickableTextureComponent(
                icon.Id,
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                null,
                null,
                this.assetHandler.CreateButtonTexture(icon),
                icon.Area,
                Game1.pixelZoom),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };

    private Texture2D GetTexture(IIcon icon) => this.assetHandler.GetTexture(icon);
}