namespace StardewMods.ExpandedStorage.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewValley.Objects;

/// <inheritdoc cref="IChestCreated" />
internal sealed class ChestCreatedEventArgs(
    Chest chest,
    GameLocation location,
    Vector2 tileLocation,
    IStorageData storageData) : EventArgs, IChestCreated
{
    /// <inheritdoc />
    public Chest Chest { get; } = chest;

    /// <inheritdoc />
    public GameLocation Location { get; } = location;

    /// <inheritdoc />
    public IStorageData StorageData { get; } = storageData;

    /// <inheritdoc />
    public Vector2 TileLocation { get; } = tileLocation;
}