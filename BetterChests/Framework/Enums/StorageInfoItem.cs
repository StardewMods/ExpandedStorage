namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Represents the info that can be displayed about storages.</summary>
[EnumExtensions]
internal enum StorageInfoItem
{
    /// <summary>The storage name.</summary>
    Name,

    /// <summary>The storage icon.</summary>
    Icon,

    /// <summary>The name or type of storage.</summary>
    Type,

    /// <summary>The location of the storage.</summary>
    Location,

    /// <summary>The position of the storage.</summary>
    Position,

    /// <summary>The farmer whose inventory contains the storage.</summary>
    Inventory,

    /// <summary>The number of item stacks and slots in the storage.</summary>
    Capacity,

    /// <summary>The total items in the storage.</summary>
    TotalItems,

    /// <summary>The number of unique items in the storage.</summary>
    UniqueItems,

    /// <summary>The total value of items in the storage.</summary>
    TotalValue,
}