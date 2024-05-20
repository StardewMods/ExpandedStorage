namespace StardewMods.CrystallineJunimoChests.Framework.Models;

/// <summary>The data model for the cost, sound, and colors.</summary>
internal sealed class DataModel
{
    /// <summary>Initializes a new instance of the <see cref="DataModel" /> class.</summary>
    /// <param name="sound">The sound.</param>
    /// <param name="colors">The colors.</param>
    public DataModel(string sound, ColorData[] colors)
    {
        this.Sound = sound;
        this.Colors = colors;
    }

    /// <summary>Gets or sets the colors.</summary>
    public ColorData[] Colors { get; set; }

    /// <summary>Gets or sets the sound.</summary>
    public string Sound { get; set; }
}