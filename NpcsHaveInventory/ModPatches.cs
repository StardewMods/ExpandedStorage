namespace StardewMods.NPCsHaveInventory;

using HarmonyLib;

/// <summary>Harmony Patches for NPCs Have Inventory.</summary>
internal sealed class ModPatches
{
    private static string modId = null!;

    /// <summary>Initializes a new instance of the <see cref="ModPatches" /> class.</summary>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ModPatches(IManifest manifest)
    {
        ModPatches.modId = manifest.UniqueID;

        // Patches
        var harmony = new Harmony(ModPatches.modId);

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
            new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.NPC_tryToReceiveActiveObject_Prefix)),
            new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.NPC_tryToReceiveActiveObject_Postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void NPC_tryToReceiveActiveObject_Postfix(NPC __instance, bool __result, Item? __state)
    {
        if (!__result)
        {
            return;
        }

        var inventory = Game1.player.team.GetOrCreateGlobalInventory($"{ModPatches.modId}-{__instance.Name}");
        inventory.Add(__state);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void NPC_tryToReceiveActiveObject_Prefix(Farmer who, out Item? __state) =>
        __state = who.ActiveObject.getOne();
}