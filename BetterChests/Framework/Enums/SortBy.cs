namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Represents the item attributes that can be used for sorting.</summary>
[EnumExtensions]
internal enum SortBy
{
    /// <summary>Sort by category.</summary>
    Category,

    /// <summary>Sort by name.</summary>
    Name,

    /// <summary>Sort by quantity.</summary>
    Quantity,

    /// <summary>Sort by quality.</summary>
    Quality,

    /// <summary>Sort by type.</summary>
    Type,
}