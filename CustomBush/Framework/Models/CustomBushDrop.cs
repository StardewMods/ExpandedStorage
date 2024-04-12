namespace StardewMods.CustomBush.Framework.Models;

using StardewMods.Common.Services.Integrations.CustomBush;
using StardewValley.GameData;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.CustomBush.ICustomBushDrop" />
public sealed class CustomBushDrop : GenericSpawnItemDataWithCondition, ICustomBushDrop
{
    /// <inheritdoc />
    public Season? Season { get; set; }

    /// <inheritdoc />
    public float Chance { get; set; } = 1f;
}