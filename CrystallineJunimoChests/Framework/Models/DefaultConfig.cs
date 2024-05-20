namespace StardewMods.CrystallineJunimoChests.Framework.Models;

using StardewMods.CrystallineJunimoChests.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public int GemCost { get; set; } = 1;
}