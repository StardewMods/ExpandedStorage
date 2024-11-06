#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.ExpandedStorage;
#else
namespace StardewMods.Common.Services.Integrations.ExpandedStorage;
#endif

using Microsoft.Xna.Framework;
using StardewValley.Objects;

/// <summary>Represents the event arguments for the ChestCreated event.</summary>
public interface IChestCreated
{
    /// <summary>Gets the chest being created.</summary>
    Chest Chest { get; }

    /// <summary>Gets the location of the chest being created.</summary>
    GameLocation Location { get; }

    /// <summary>Gets the expanded storage data for the chest being created.</summary>
    IStorageData StorageData { get; }

    /// <summary>Gets the tile location of the chest being created.</summary>
    Vector2 TileLocation { get; }
}