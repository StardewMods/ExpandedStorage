#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Enums;
#else

namespace StardewMods.Common.Enums;
#endif

using NetEscapades.EnumGenerators;

/// <summary>Represents the item attributes that can be used for searching.</summary>
[EnumExtensions]
internal enum ItemAttribute
{
    /// <summary>Any attribute.</summary>
    Any,

    /// <summary>Category name.</summary>
    Category,

    /// <summary>Display name.</summary>
    Name,

    /// <summary>Stack size.</summary>
    Quantity,

    /// <summary>Quality name.</summary>
    Quality,

    /// <summary>Context tags.</summary>
    Tags,
}