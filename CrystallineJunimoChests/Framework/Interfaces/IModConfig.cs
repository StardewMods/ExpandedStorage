namespace StardewMods.CrystallineJunimoChests.Framework.Interfaces;

/// <summary>Mod Config options for Crystalline Junimo Chests.</summary>
internal interface IModConfig
{
    /// <summary>Gets the amount of gems required to change the color.</summary>
    public int GemCost { get; }
}