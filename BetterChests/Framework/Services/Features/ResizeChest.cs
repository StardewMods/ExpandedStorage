namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Objects;

/// <summary>Expand the capacity of chests and add scrolling to access extra items.</summary>
internal sealed class ResizeChest : BaseFeature<ResizeChest>
{
    private static ResizeChest instance = null!;

    private readonly ContainerFactory containerFactory;
    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="ResizeChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ResizeChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IPatchManager patchManager)
        : base(eventManager, log, manifest, modConfig)
    {
        ResizeChest.instance = this;
        this.containerFactory = containerFactory;
        this.patchManager = patchManager;

        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)),
                AccessTools.DeclaredMethod(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)),
                PatchType.Postfix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.ResizeChest != ChestMenuOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.patchManager.Patch(this.UniqueId);

    /// <inheritdoc />
    protected override void Deactivate() => this.patchManager.Unpatch(this.UniqueId);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (!ResizeChest.instance.containerFactory.TryGetOne(__instance, out var container))
        {
            return;
        }

        __result = Math.Max(
            container.Items.Count, // Guarantee space for existing items
            Math.Max(
                (int)container.ResizeChest, // Guarantee space for menu size
                container.ResizeChestCapacity switch
                {
                    // Always allocate +1 space for unlimited storage
                    < 0 => container.Items.Count + 1,

                    // Allocate assigned space
                    > 0 => container.ResizeChestCapacity,

                    // Allocate vanilla space
                    _ => __result,
                }));
    }
}