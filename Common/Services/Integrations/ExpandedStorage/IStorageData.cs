#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.ExpandedStorage;
#else
namespace StardewMods.Common.Services.Integrations.ExpandedStorage;
#endif

using Microsoft.Xna.Framework;

/// <summary>Data for an Expanded Storage chest.</summary>
public interface IStorageData
{
    /// <summary>Gets or sets the sound to play when the lid closing animation plays.</summary>
    public string CloseNearbySound { get; set; }

    /// <summary>Gets or sets the number of frames in the lid animation.</summary>
    public int Frames { get; set; }

    /// <summary>Gets or sets the global inventory id.</summary>
    public string? GlobalInventoryId { get; set; }

    /// <summary>Gets or sets a value indicating whether the storage is a fridge.</summary>
    public bool IsFridge { get; set; }

    /// <summary>Gets or sets any mod data that should be added to the chest on creation.</summary>
    public Dictionary<string, string>? ModData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the storage will play it's lid opening animation when the player is
    /// nearby.
    /// </summary>
    public bool OpenNearby { get; set; }

    /// <summary>Gets or sets the sound to play when the lid opening animation plays.</summary>
    public string OpenNearbySound { get; set; }

    /// <summary>Gets or sets the sound to play when the storage is opened.</summary>
    public string OpenSound { get; set; }

    /// <summary>Gets or sets the sound to play when storage is placed.</summary>
    public string PlaceSound { get; set; }

    /// <summary>Gets or sets a value indicating whether player color is enabled.</summary>
    public bool PlayerColor { get; set; }

    /// <summary>Gets or sets a value to override the texture.</summary>
    public string TextureOverride { get; set; }

    /// <summary>Gets or sets a color to apply to the tinted layer.</summary>
    public Color TintOverride { get; set; }
}