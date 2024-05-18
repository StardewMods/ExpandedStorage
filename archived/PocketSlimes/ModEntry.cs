namespace StardewMods.PocketSlimes;

using StardewMods.Common.Helpers;
using StardewMods.PocketSlimes.Framework;

/// <inheritdoc />
public sealed class ModEntry : StardewModdingAPI.Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        ModPatches.Init(this.ModManifest);
    }
}