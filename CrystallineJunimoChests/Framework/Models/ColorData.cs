namespace StardewMods.CrystallineJunimoChests.Framework.Models;

using Microsoft.Xna.Framework;

/// <summary>The data model for the color and items.</summary>
internal sealed class ColorData
{
    /// <summary>Initializes a new instance of the <see cref="ColorData" /> class.</summary>
    /// <param name="name">The color name.</param>
    /// <param name="item">The item.</param>
    /// <param name="tint">The tint.</param>
    public ColorData(string name, string item, string tint)
    {
        this.Name = name;
        this.Item = item;
        this.Tint = tint;
        this.Color = Utility.StringToColor(this.Tint) ?? Color.Black;
    }

    /// <summary>Gets the color.</summary>
    public Color Color { get; }

    /// <summary>Gets or sets the item required to change to the color.</summary>
    public string Item { get; set; }

    /// <summary>Gets or sets the name of the color.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the color.</summary>
    public string Tint { get; set; }
}