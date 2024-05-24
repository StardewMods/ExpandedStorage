namespace StardewMods.FauxCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class IconRegistry : IIconRegistry
{
    private static readonly Dictionary<string, IconRegistry> Registries = [];

    private readonly IAssetHandlerExtension assetHandler;
    private readonly Dictionary<string, Icon> icons = [];
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="IconRegistry" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public IconRegistry(IAssetHandlerExtension assetHandler, IManifest manifest)
    {
        IconRegistry.Registries.Add(manifest.UniqueID, this);
        this.assetHandler = assetHandler;
        this.manifest = manifest;
    }

    /// <summary>Adds an icon to the icon registry.</summary>
    /// <param name="id">The unique identifier of the icon.</param>
    /// <param name="path">The icon texture path.</param>
    /// <param name="area">The icon source area.</param>
    /// <param name="source">The icon source mod.</param>
    /// <returns>The icon.</returns>
    public Icon Add(string id, string path, Rectangle area, string source)
    {
        var icon = new Icon(this.GetTexture, this.CreateComponent)
        {
            Area = area,
            Id = id,
            Path = path,
            Source = source,
        };

        if (this.icons.TryAdd(id, icon))
        {
            this.assetHandler.AddAsset(icon);
        }

        return icon;
    }

    /// <inheritdoc />
    public void AddIcon(string id, string path, Rectangle area)
    {
        if (!this.icons.TryGetValue(id, out var icon))
        {
            icon = this.Add(id, path, area, this.manifest.UniqueID);
        }

        Log.Trace("Registering icon: {0}", id);
        icon.Path = path;
        icon.Area = area;
    }

    /// <inheritdoc />
    public IEnumerable<IIcon> GetIcons(string? modId = null) =>
        modId is null
            ? IconRegistry.Registries.Values.SelectMany(reg => reg.icons.Values)
            : IconRegistry.Registries.TryGetValue(modId, out var registry)
                ? registry.icons.Values
                : Enumerable.Empty<IIcon>();

    /// <inheritdoc />
    public IIcon Icon(string id) =>
        new IconWrapper(
            () => this.TryGetIcon(id, out var icon)
                ? icon
                : throw new KeyNotFoundException($"No icon found with the id: {id}."));

    /// <inheritdoc />
    public IIcon Icon(VanillaIcon icon) =>
        new IconWrapper(
            () => IconRegistry.Registries[Mod.Id].icons.TryGetValue(icon.ToStringFast(), out var vanillaIcon)
                ? vanillaIcon
                : throw new KeyNotFoundException($"No icon found with the id: {icon}."));

    /// <inheritdoc />
    public bool TryGetIcon(string id, [NotNullWhen(true)] out IIcon? icon)
    {
        icon = null;

        // Internal icons
        if (this.icons.TryGetValue(id, out var internalIcon))
        {
            icon = internalIcon;
            return true;
        }

        // All icons
        var parts = id.Split('/');
        if (parts.Length < 2)
        {
            return false;
        }

        var modId = parts[0];
        var subId = string.Join(string.Empty, parts[1..]);
        if (!IconRegistry.Registries.TryGetValue(modId, out var registry))
        {
            registry = IconRegistry.Registries[Mod.Id];
        }

        if (!registry.icons.TryGetValue(subId, out var externalIcon)
            && !registry.icons.TryGetValue(id, out externalIcon))
        {
            return false;
        }

        icon = externalIcon;
        return true;
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