﻿namespace StardewMods.BetterChests.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Records;
using StardewMods.BetterChests.Records;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Extensions for the PlacedChest record to retrieve the associated Chest object.
/// </summary>
internal static class PlacedChestExtensions
{
    /// <summary>
    ///     Gets or sets a function that returns accessible game locations for the current player.
    /// </summary>
    public static Func<IEnumerable<GameLocation>> GetAccessibleLocations { get; set; }

    /// <summary>
    ///     Gets the Chest associated with the PlacedChest.
    /// </summary>
    /// <param name="placedChest">The PlacedChest to get the Chest for.</param>
    /// <returns>Returns a Chest if it is accessible to the player.</returns>
    public static Chest GetChest(this PlacedChest placedChest)
    {
        var placedObject = placedChest.GetPlacedObject();
        var (location, position) = placedObject;
        return location switch
        {
            FarmHouse farmHouse when farmHouse.fridgePosition.ToVector2().Equals(position) => farmHouse.fridge.Value,
            not null => placedObject.Object as Chest,
            _ => null,
        };
    }

    /// <summary>
    ///     Gets an object placed in a location at a position.
    /// </summary>
    /// <param name="placedChest">The placed Chest to get the object for.</param>
    /// <returns>Returns the PlacedObject representation of the object.</returns>
    public static PlacedObject GetPlacedObject(this PlacedChest placedChest)
    {
        return new(PlacedChestExtensions.GetAccessibleLocations().FirstOrDefault(location => location.NameOrUniqueName == placedChest.LocationName), new(placedChest.X, placedChest.Y));
    }
}