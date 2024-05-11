namespace StardewMods.Common.Services.Integrations.BetterChests;

using NetEscapades.EnumGenerators;

/// <summary>Represents the hierarchy of storage options.</summary>
[EnumExtensions]
public enum StorageOption
{
    /// <summary>The individual options on a storage instance.</summary>
    Individual = 0,

    /// <summary>The base options for each type.</summary>
    Type = 5,

    /// <summary>The global default options.</summary>
    Global = 10,
}