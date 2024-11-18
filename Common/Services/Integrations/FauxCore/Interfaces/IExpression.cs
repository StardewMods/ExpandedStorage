#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
#else

namespace StardewMods.Common.Services.Integrations.FauxCore;
#endif

using System.Collections.Immutable;
using StardewValley.Inventories;

/// <summary>Represents an expression.</summary>
public interface IExpression : IComparer<Item>, IEquatable<Item>, IEquatable<IInventory>, IEquatable<string>
{
    /// <summary>Gets the type of expression.</summary>
    ExpressionType ExpressionType { get; }

    /// <summary>Gets a value indicating whether the expression is valid.</summary>
    bool IsValid { get; }

    /// <summary>Gets the text value.</summary>
    string Text { get; }

    /// <summary>Gets or sets the sub-expressions.</summary>
    IImmutableList<IExpression> Expressions { get; set; }

    /// <summary>Gets or sets the parent expression.</summary>
    IExpression? Parent { get; set; }

    /// <summary>Gets or sets the associated value.</summary>
    string Term { get; set; }
}