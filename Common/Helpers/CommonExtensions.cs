namespace StardewMods.Common.Helpers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.Mods;

/// <summary>Common extension methods.</summary>
internal static class CommonExtensions
{
    /// <summary>Generate a box of coordinates centered at a specified point with a given radius.</summary>
    /// <param name="center">The center point of the box.</param>
    /// <param name="radius">The radius of the box.</param>
    /// <returns>An enumerable collection of Vector2 coordinates representing the points in the box.</returns>
    public static IEnumerable<Vector2> Box(this Vector2 center, int radius)
    {
        for (var x = (int)(center.X - radius); x <= center.X + radius; ++x)
        {
            for (var y = (int)(center.Y - radius); y <= center.Y + radius; ++y)
            {
                yield return new Vector2(x, y);
            }
        }
    }

    /// <summary>Copy storage options.</summary>
    /// <param name="from">The storage options to copy from.</param>
    /// <param name="to">The storage options to copy to.</param>
    public static void CopyTo(this IStorageOptions from, IStorageOptions to) =>
        from.ForEachOption(
            (name, option) =>
            {
                switch (option)
                {
                    case FeatureOption featureOption:
                        to.SetOption(name, featureOption);
                        return;

                    case RangeOption rangeOption:
                        to.SetOption(name, rangeOption);
                        return;

                    case ChestMenuOption chestMenuOption:
                        to.SetOption(name, chestMenuOption);
                        return;

                    case StashPriority stashPriority:
                        to.SetOption(name, stashPriority);
                        return;

                    case string stringValue:
                        to.SetOption(name, stringValue);
                        return;

                    case int intValue:
                        to.SetOption(name, intValue);
                        return;
                }
            });

    /// <summary>Executes the specified action for each option in the class.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="action">The action to be performed for each option.</param>
    public static void ForEachOption(this IStorageOptions options, Action<string, object> action)
    {
        action(nameof(options.AccessChest), options.AccessChest);
        action(nameof(options.AccessChestPriority), options.AccessChestPriority);
        action(nameof(options.AutoOrganize), options.AutoOrganize);
        action(nameof(options.CarryChest), options.CarryChest);
        action(nameof(options.CategorizeChest), options.CategorizeChest);
        action(nameof(options.CategorizeChestBlockItems), options.CategorizeChestBlockItems);
        action(nameof(options.CategorizeChestSearchTerm), options.CategorizeChestSearchTerm);
        action(nameof(options.CategorizeChestIncludeStacks), options.CategorizeChestIncludeStacks);
        action(nameof(options.ChestFinder), options.ChestFinder);
        action(nameof(options.CollectItems), options.CollectItems);
        action(nameof(options.ConfigureChest), options.ConfigureChest);
        action(nameof(options.CookFromChest), options.CookFromChest);
        action(nameof(options.CraftFromChest), options.CraftFromChest);
        action(nameof(options.CraftFromChestDistance), options.CraftFromChestDistance);
        action(nameof(options.HslColorPicker), options.HslColorPicker);
        action(nameof(options.InventoryTabs), options.InventoryTabs);
        action(nameof(options.OpenHeldChest), options.OpenHeldChest);
        action(nameof(options.ResizeChest), options.ResizeChest);
        action(nameof(options.ResizeChestCapacity), options.ResizeChestCapacity);
        action(nameof(options.SearchItems), options.SearchItems);
        action(nameof(options.ShopFromChest), options.ShopFromChest);
        action(nameof(options.SortInventory), options.SortInventory);
        action(nameof(options.SortInventoryBy), options.SortInventoryBy);
        action(nameof(options.StashToChest), options.StashToChest);
        action(nameof(options.StashToChestDistance), options.StashToChestDistance);
        action(nameof(options.StashToChestPriority), options.StashToChestPriority);
        action(nameof(options.StorageIcon), options.StorageIcon);
        action(nameof(options.StorageInfo), options.StorageInfo);
        action(nameof(options.StorageInfoHover), options.StorageInfoHover);
        action(nameof(options.StorageName), options.StorageName);
    }

    /// <summary>Tries to parse the specified string value as a boolean and returns the result.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="defaultValue">The default value to return if the value cannot be parsed as a boolean.</param>
    /// <returns>The boolean value, or the default value if the value is not a valid boolean.</returns>
    public static bool GetBool(this string value, bool defaultValue = false) =>
        !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out var boolValue) ? boolValue : defaultValue;

    /// <summary>Retrieves a boolean value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="dictionary">The dictionary to retrieve the boolean value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid boolean. </param>
    /// <returns>The boolean value associated with the key, or the default value.</returns>
    public static bool GetBool(this IDictionary<string, string> dictionary, string key, bool defaultValue = false) =>
        dictionary.TryGetValue(key, out var value) ? value.GetBool(defaultValue) : defaultValue;

    /// <summary>Retrieves a boolean value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="modData">The mod data dictionary to retrieve the boolean value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid boolean. </param>
    /// <returns>The boolean value associated with the key, or the default value.</returns>
    public static bool GetBool(this ModDataDictionary modData, string key, bool defaultValue = false) =>
        modData.TryGetValue(key, out var value) ? value.GetBool(defaultValue) : defaultValue;

    /// <summary>Tries to parse the specified string value as an integer and returns the result.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="defaultValue">The default value to return if the value cannot be parsed as an integer.</param>
    /// <returns>The integer value, or the default value if the value is not a valid integer.</returns>
    public static int GetInt(this string value, int defaultValue = 0) =>
        !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var intValue) ? intValue : defaultValue;

    /// <summary>Retrieves an integer value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="modData">The mod data dictionary to retrieve the integer value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid integer.</param>
    /// <returns>The integer value associated with the key, or the default value.</returns>
    public static int GetInt(this ModDataDictionary modData, string key, int defaultValue = 0) =>
        modData.TryGetValue(key, out var value) ? value.GetInt(defaultValue) : defaultValue;

    /// <summary>Retrieves an integer value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="dictionary">The dictionary to retrieve the integer value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid integer.</param>
    /// <returns>The integer value associated with the key, or the default value.</returns>
    public static int GetInt(this IDictionary<string, string> dictionary, string key, int defaultValue = 0) =>
        dictionary.TryGetValue(key, out var value) ? value.GetInt(defaultValue) : defaultValue;

    /// <summary>Invokes all event handlers for an event.</summary>
    /// <param name="eventHandler">The event.</param>
    /// <param name="source">The source.</param>
    public static void InvokeAll(this EventHandler? eventHandler, object source)
    {
        if (eventHandler is null)
        {
            return;
        }

        foreach (var handler in eventHandler.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(source, null);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    /// <summary>Invokes all event handlers for an event.</summary>
    /// <param name="eventHandler">The event.</param>
    /// <param name="source">The source.</param>
    /// <param name="param">The event parameters.</param>
    /// <typeparam name="T">The event handler type.</typeparam>
    public static void InvokeAll<T>(this EventHandler<T>? eventHandler, object source, T param)
    {
        if (eventHandler is null)
        {
            return;
        }

        foreach (var @delegate in eventHandler.GetInvocationList())
        {
            var handler = (EventHandler<T>)@delegate;
            try
            {
                handler(source, param);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    /// <summary>Maps a float value from one range to the same proportional value in another integer range.</summary>
    /// <param name="value">The float value to map.</param>
    /// <param name="sourceRange">The source range of the float value.</param>
    /// <param name="targetRange">The target range to map to.</param>
    /// <returns>The integer value.</returns>
    public static int Remap(this float value, Range<float> sourceRange, Range<int> targetRange) =>
        targetRange.Clamp(
            (int)(targetRange.Minimum
                + ((targetRange.Maximum - targetRange.Minimum)
                    * ((value - sourceRange.Minimum) / (sourceRange.Maximum - sourceRange.Minimum)))));

    /// <summary>Maps an integer value from one range to the same proportional value in another float range.</summary>
    /// <param name="value">The integer value to map.</param>
    /// <param name="sourceRange">The source range of the integer value.</param>
    /// <param name="targetRange">The target range to map to.</param>
    /// <returns>The float value.</returns>
    public static float Remap(this int value, Range<int> sourceRange, Range<float> targetRange) =>
        targetRange.Clamp(
            targetRange.Minimum
            + ((targetRange.Maximum - targetRange.Minimum)
                * ((float)(value - sourceRange.Minimum) / (sourceRange.Maximum - sourceRange.Minimum))));

    /// <summary>Rounds an int up to the next int by an interval.</summary>
    /// <param name="i">The integer to round up from.</param>
    /// <param name="d">The interval to round up to.</param>
    /// <returns>Returns the rounded value.</returns>
    public static int RoundUp(this int i, int d = 1) => (int)(d * Math.Ceiling((float)i / d));

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, FeatureOption value)
    {
        switch (name)
        {
            case nameof(options.AutoOrganize):
                options.AutoOrganize = value;
                return;
            case nameof(options.CarryChest):
                options.CarryChest = value;
                return;
            case nameof(options.CategorizeChest):
                options.CategorizeChest = value;
                return;
            case nameof(options.CategorizeChestBlockItems):
                options.CategorizeChestBlockItems = value;
                return;
            case nameof(options.CategorizeChestIncludeStacks):
                options.CategorizeChestIncludeStacks = value;
                return;
            case nameof(options.ChestFinder):
                options.ChestFinder = value;
                return;
            case nameof(options.CollectItems):
                options.CollectItems = value;
                return;
            case nameof(options.ConfigureChest):
                options.ConfigureChest = value;
                return;
            case nameof(options.HslColorPicker):
                options.HslColorPicker = value;
                return;
            case nameof(options.InventoryTabs):
                options.InventoryTabs = value;
                return;
            case nameof(options.OpenHeldChest):
                options.OpenHeldChest = value;
                return;
            case nameof(options.SearchItems):
                options.SearchItems = value;
                return;
            case nameof(options.ShopFromChest):
                options.ShopFromChest = value;
                return;
            case nameof(options.SortInventory):
                options.SortInventory = value;
                return;
            case nameof(options.StorageInfo):
                options.StorageInfo = value;
                return;
            case nameof(options.StorageInfoHover):
                options.StorageInfoHover = value;
                return;
            default: throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, RangeOption value)
    {
        switch (name)
        {
            case nameof(options.AccessChest):
                options.AccessChest = value;
                return;
            case nameof(options.CookFromChest):
                options.CookFromChest = value;
                return;
            case nameof(options.CraftFromChest):
                options.CraftFromChest = value;
                return;
            case nameof(options.StashToChest):
                options.StashToChest = value;
                return;
            default: throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, string value)
    {
        switch (name)
        {
            case nameof(options.CategorizeChestSearchTerm):
                options.CategorizeChestSearchTerm = value;
                return;
            case nameof(options.SortInventoryBy):
                options.SortInventoryBy = value;
                return;
            case nameof(options.StorageIcon):
                options.StorageIcon = value;
                return;
            case nameof(options.StorageName):
                options.StorageName = value;
                return;
            default: throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, int value)
    {
        switch (name)
        {
            case nameof(options.AccessChestPriority):
                options.AccessChestPriority = value;
                return;
            case nameof(options.CraftFromChestDistance):
                options.CraftFromChestDistance = value;
                return;
            case nameof(options.ResizeChestCapacity):
                options.ResizeChestCapacity = value;
                return;
            case nameof(options.StashToChestDistance):
                options.StashToChestDistance = value;
                return;
            default: throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, ChestMenuOption value)
    {
        switch (name)
        {
            case nameof(options.ResizeChest):
                options.ResizeChest = value;
                return;
            default: throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, StashPriority value)
    {
        switch (name)
        {
            case nameof(options.StashToChestPriority):
                options.StashToChestPriority = value;
                return;
            default: throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Shuffles a list randomly.</summary>
    /// <param name="source">The list to shuffle.</param>
    /// <typeparam name="T">The list type.</typeparam>
    /// <returns>Returns a shuffled list.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.Shuffle(new Random());

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns><c>true</c> if the options exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out FeatureOption value)
    {
        value = name switch
        {
            nameof(options.AutoOrganize) => options.AutoOrganize,
            nameof(options.CarryChest) => options.CarryChest,
            nameof(options.CategorizeChest) => options.CategorizeChest,
            nameof(options.CategorizeChestBlockItems) => options.CategorizeChestBlockItems,
            nameof(options.CategorizeChestIncludeStacks) => options.CategorizeChestIncludeStacks,
            nameof(options.ChestFinder) => options.ChestFinder,
            nameof(options.CollectItems) => options.CollectItems,
            nameof(options.ConfigureChest) => options.ConfigureChest,
            nameof(options.HslColorPicker) => options.HslColorPicker,
            nameof(options.InventoryTabs) => options.InventoryTabs,
            nameof(options.OpenHeldChest) => options.OpenHeldChest,
            nameof(options.SearchItems) => options.SearchItems,
            nameof(options.ShopFromChest) => options.ShopFromChest,
            nameof(options.SortInventory) => options.SortInventory,
            nameof(options.StorageInfo) => options.StorageInfo,
            nameof(options.StorageInfoHover) => options.StorageInfoHover,
            _ => throw new ArgumentOutOfRangeException(name),
        };

        return value is not default(FeatureOption);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns><c>true</c> if the options exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out RangeOption value)
    {
        value = name switch
        {
            nameof(options.AccessChest) => options.AccessChest,
            nameof(options.CookFromChest) => options.CookFromChest,
            nameof(options.CraftFromChest) => options.CraftFromChest,
            nameof(options.StashToChest) => options.StashToChest,
            _ => throw new ArgumentOutOfRangeException(name),
        };

        return value is not default(RangeOption);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns><c>true</c> if the options exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out ChestMenuOption value)
    {
        value = name switch
        {
            nameof(options.ResizeChest) => options.ResizeChest, _ => throw new ArgumentOutOfRangeException(name),
        };

        return value is not default(ChestMenuOption);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns><c>true</c> if the options exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out StashPriority value)
    {
        value = name switch
        {
            nameof(options.StashToChestPriority) => options.StashToChestPriority,
            _ => throw new ArgumentOutOfRangeException(name),
        };

        return value is not default(StashPriority);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns><c>true</c> if the options exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out string value)
    {
        value = name switch
        {
            nameof(options.CategorizeChestSearchTerm) => options.CategorizeChestSearchTerm,
            nameof(options.SortInventoryBy) => options.SortInventoryBy,
            nameof(options.StorageIcon) => options.StorageIcon,
            nameof(options.StorageName) => options.StorageName,
            _ => throw new ArgumentOutOfRangeException(name),
        };

        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns><c>true</c> if the options exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out int value)
    {
        value = name switch
        {
            nameof(options.AccessChestPriority) => options.AccessChestPriority,
            nameof(options.CraftFromChestDistance) => options.CraftFromChestDistance,
            nameof(options.ResizeChestCapacity) => options.ResizeChestCapacity,
            nameof(options.StashToChestDistance) => options.StashToChestDistance,
            _ => throw new ArgumentOutOfRangeException(name),
        };

        return value != 0;
    }

    /// <summary>Tests whether the player is within range of the location.</summary>
    /// <param name="range">The range.</param>
    /// <param name="distance">The distance in tiles to the player.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="position">The coordinates.</param>
    /// <returns><c>true</c> if the location is within range; otherwise, <c>false</c>.</returns>
    public static bool WithinRange(this RangeOption range, int distance, object parent, Vector2 position) =>
        range switch
        {
            RangeOption.World => true,
            RangeOption.Inventory when parent is Farmer farmer && farmer.Equals(Game1.player) => true,
            RangeOption.Default or RangeOption.Disabled or RangeOption.Inventory => false,
            RangeOption.Location when parent is GameLocation location && !location.Equals(Game1.currentLocation) =>
                false,
            RangeOption.Location when distance == -1 => true,
            RangeOption.Location when Math.Abs(position.X - Game1.player.Tile.X)
                + Math.Abs(position.Y - Game1.player.Tile.Y)
                <= distance => true,
            _ => false,
        };

    private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (rng is null)
        {
            throw new ArgumentNullException(nameof(rng));
        }

        return source.ShuffleIterator(rng);
    }

    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
    {
        var buffer = source.ToList();
        for (var i = 0; i < buffer.Count; ++i)
        {
            var j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}