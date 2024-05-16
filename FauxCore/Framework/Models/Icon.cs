namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <inheritdoc />
public sealed class Icon : IIcon
{
    private readonly Func<IIcon, IconStyle, ClickableTextureComponent> getComponent;
    private readonly Func<IIcon, IconStyle, Texture2D> getTexture;

    /// <summary>Initializes a new instance of the <see cref="Icon" /> class.</summary>
    /// <param name="getTexture">A function that returns the button texture.</param>
    /// <param name="getComponent">A function that return a new button.</param>
    public Icon(
        Func<IIcon, IconStyle, Texture2D> getTexture,
        Func<IIcon, IconStyle, ClickableTextureComponent> getComponent)
    {
        this.getTexture = getTexture;
        this.getComponent = getComponent;
    }

    /// <inheritdoc />
    public Rectangle Area { get; set; } = Rectangle.Empty;

    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc />
    public ClickableTextureComponent GetComponent(IconStyle style) => this.getComponent(this, style);

    /// <inheritdoc />
    public Texture2D GetTexture(IconStyle style) => this.getTexture(this, style);
}