namespace StardewMods.NPCsHaveInventory;

using StardewModdingAPI.Events;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        _ = new ModPatches(this.ModManifest);

        // Events
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) =>
        Utility.ForEachVillager(
            villager =>
            {
                Game1.player.team.GetOrCreateGlobalInventory($"{this.ModManifest.UniqueID}-{villager.Name}");
                return true;
            });
}