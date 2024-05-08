namespace StardewMods.Common.Services.Integrations.BetterChests.Enums;

using NetEscapades.EnumGenerators;

/// <summary>The possible values for Stash to Chest Priority.</summary>
[EnumExtensions]
public enum StashPriority
{
    /// <summary>Represents the default priority.</summary>
    Default = 0,

    /// <summary>Represents the lowest priority.</summary>
    Lowest = -3,

    /// <summary>Represents a lower priority.</summary>
    Lower = -2,

    /// <summary>Represents a low priority.</summary>
    Low = -1,

    /// <summary>Represents a high priority.</summary>
    High = 1,

    /// <summary>Represents a higher priority.</summary>
    Higher = 2,

    /// <summary>Represents the highest priority.</summary>
    Highest = 3,
}