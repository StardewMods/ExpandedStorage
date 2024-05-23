#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
#else
namespace StardewMods.Common.Services.Integrations.FauxCore;
#endif

using NetEscapades.EnumGenerators;

/// <summary>The types of expressions.</summary>
[EnumExtensions]
public enum ExpressionType
{
    /// <summary>An expression in which all sub-expressions must be true.</summary>
    All,

    /// <summary>An expression in which any sub-expression must be true.</summary>
    Any,

    /// <summary>An expression where the first sub-expression must match the second.</summary>
    Comparable,

    /// <summary>An expression that dynamically pulls an attribute.</summary>
    Dynamic,

    /// <summary>An expression where its sub-expression must not be true.</summary>
    Not,

    /// <summary>An expression that wraps a static term in quotes.</summary>
    Quoted,

    /// <summary>An expression that matches an Item's internal attributes against the string.</summary>
    Static,
}