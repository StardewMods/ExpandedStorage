namespace StardewMods.Common.Services.Integrations.FauxCore;

using StardewValley.Inventories;

/// <summary>Represents a search expression.</summary>
public interface IExpression : IComparer<Item>, IEquatable<Item>, IEquatable<IInventory>, IEquatable<string>
{
    /// <summary>Gets the sub-expressions.</summary>
    IEnumerable<IExpression> Expressions { get; }

    /// <summary>Gets the type of expression.</summary>
    ExpressionType ExpressionType { get; }

    /// <summary>Gets the associated value.</summary>
    string? Term { get; }
}