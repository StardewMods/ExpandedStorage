namespace StardewMods.GarbageDay.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services;

/// <summary>Represents a pending garbage can object on a map.</summary>
internal sealed class FoundGarbageCan
{
    private bool isValid = true;

    /// <summary>Initializes a new instance of the <see cref="FoundGarbageCan" /> class.</summary>
    /// <param name="whichCan">The name of the garbage can.</param>
    /// <param name="assetName">The asset name of the map containing the garbage can.</param>
    /// <param name="x">The x-coordinate of the garbage can.</param>
    /// <param name="y">The y-coordinate of the garbage can.</param>
    public FoundGarbageCan(string whichCan, IAssetName assetName, int x, int y)
    {
        this.WhichCan = whichCan;
        this.AssetName = assetName;
        this.TilePosition = new Vector2(x, y);
    }

    /// <summary>Gets the asset name of the map containing the garbage can.</summary>
    public IAssetName AssetName { get; }

    /// <summary>Gets or sets a value indicating whether the garbage can is valid.</summary>
    public bool IsValid
    {
        get => this.isValid;
        set
        {
            if (this.isValid && !value)
            {
                Log.Trace("Failed to create garbage can: {0}", this.WhichCan);
            }

            this.isValid = value;
        }
    }

    /// <summary>Gets the tile position of the garbage can.</summary>
    public Vector2 TilePosition { get; }

    /// <summary>Gets the name of the garbage can.</summary>
    public string WhichCan { get; }
}