namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Gets actions that can be performed in the expression editor.</summary>
[EnumExtensions]
internal enum ExpressionChange
{
    /// <summary>Add a new group expression.</summary>
    AddGroup,

    /// <summary>Add a new not expression.</summary>
    AddNot,

    /// <summary>Add a new expression term.</summary>
    AddTerm,

    /// <summary>Remove an expression.</summary>
    Remove,

    /// <summary>Toggle an expression group type.</summary>
    ToggleGroup,

    /// <summary>Update an expression term.</summary>
    UpdateTerm,
}