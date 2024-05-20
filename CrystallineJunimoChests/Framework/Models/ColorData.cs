namespace StardewMods.CrystallineJunimoChests.Framework.Models;

/// <summary>The data model for the color and items.</summary>
internal sealed class ColorData
{
    /// <summary>Initializes a new instance of the <see cref="ColorData" /> class.</summary>
    /// <param name="name">The color name.</param>
    /// <param name="item">The item.</param>
    /// <param name="color">The color.</param>
    public ColorData(string name, string item, string color)
    {
        this.Name = name;
        this.Item = item;
        this.Color = color;
    }

    /// <summary>Gets or sets the color.</summary>
    public string Color { get; set; }

    /// <summary>Gets or sets the item required to change to the color.</summary>
    public string Item { get; set; }

    /// <summary>Gets or sets the name of the color.</summary>
    public string Name { get; set; }
}