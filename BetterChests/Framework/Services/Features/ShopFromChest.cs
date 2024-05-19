namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>Shop using items from placed chests and chests in the farmer's inventory.</summary>
internal sealed class ShopFromChest : BaseFeature<ShopFromChest>
{
    private static ShopFromChest instance = null!;

    private readonly ContainerFactory containerFactory;
    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="ShopFromChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ShopFromChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IModConfig modConfig,
        IPatchManager patchManager)
        : base(eventManager, modConfig)
    {
        ShopFromChest.instance = this;
        this.containerFactory = containerFactory;
        this.patchManager = patchManager;

        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(CarpenterMenu), nameof(CarpenterMenu.ConsumeResources)),
                AccessTools.DeclaredMethod(
                    typeof(ShopFromChest),
                    nameof(ShopFromChest.CarpenterMenu_ConsumeResources_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(CarpenterMenu),
                    nameof(CarpenterMenu.DoesFarmerHaveEnoughResourcesToBuild)),
                AccessTools.DeclaredMethod(
                    typeof(ShopFromChest),
                    nameof(ShopFromChest.CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(CarpenterMenu), nameof(CarpenterMenu.draw)),
                AccessTools.DeclaredMethod(typeof(ShopFromChest), nameof(ShopFromChest.CarpenterMenu_draw_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ShopMenu), nameof(ShopMenu.ConsumeTradeItem)),
                AccessTools.DeclaredMethod(
                    typeof(ShopFromChest),
                    nameof(ShopFromChest.ShopMenu_ConsumeTradeItem_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ShopMenu), nameof(ShopMenu.HasTradeItem)),
                AccessTools.DeclaredMethod(typeof(ShopFromChest), nameof(ShopFromChest.ShopMenu_HasTradeItem_postfix)),
                PatchType.Postfix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.ShopFromChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.patchManager.Patch(this.UniqueId);

    /// <inheritdoc />
    protected override void Deactivate() => this.patchManager.Unpatch(this.UniqueId);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool CarpenterMenu_ConsumeResources_prefix(CarpenterMenu __instance)
    {
        var blueprint = __instance.Blueprint;
        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();

        foreach (var item in __instance.ingredients)
        {
            var amount = Game1.player.Items.ReduceId(item.QualifiedItemId, item.Stack);
            if (amount == item.Stack)
            {
                continue;
            }

            var remaining = item.Stack - amount;
            foreach (var container in containers)
            {
                amount = container.Items.ReduceId(item.QualifiedItemId, remaining);
                remaining -= amount;
                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        Game1.player.Money -= blueprint.BuildCost;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_postfix(
        CarpenterMenu __instance,
        ref bool __result)
    {
        if (__result)
        {
            return;
        }

        var blueprint = __instance.Blueprint;
        if (blueprint.BuildCost < 0 || Game1.player.Money < blueprint.BuildCost)
        {
            return;
        }

        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var item in __instance.ingredients)
        {
            var amount = Game1.player.Items.CountId(item.QualifiedItemId);
            var remaining = item.Stack - amount;
            if (remaining <= 0)
            {
                continue;
            }

            foreach (var container in containers)
            {
                amount = container.Items.CountId(item.QualifiedItemId);
                remaining -= amount;
                if (remaining < 0)
                {
                    break;
                }
            }

            if (remaining > 0)
            {
                return;
            }
        }

        __result = true;
    }

    private static IEnumerable<CodeInstruction>
        CarpenterMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions) =>
        instructions.MethodReplacer(
            AccessTools.DeclaredMethod(typeof(Inventory), nameof(Inventory.ContainsId), [typeof(string), typeof(int)]),
            AccessTools.DeclaredMethod(typeof(ShopFromChest), nameof(ShopFromChest.ContainsId)));

    private static bool ContainsId(Inventory items, string itemId, int minimum)
    {
        var amount = Game1.player.Items.CountId(itemId);
        var remaining = minimum - amount;
        if (remaining <= 0)
        {
            return true;
        }

        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var container in containers)
        {
            amount = container.Items.CountId(itemId);
            remaining -= amount;
            if (remaining < 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool DefaultPredicate(IStorageContainer container) =>
        container is not FarmerContainer
        && container.ShopFromChest is not FeatureOption.Disabled
        && container.Items.Count > 0
        && !ShopFromChest.instance.Config.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
        && !(ShopFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
            && Game1.player.currentLocation is MineShaft)
        && container.CraftFromChest.WithinRange(
            container.CraftFromChestDistance,
            container.Location,
            container.TileLocation);

    private static bool ShopMenu_ConsumeTradeItem_prefix(string itemId, int count)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId is "(O)858" or "(O)73" || Game1.player.Items.ContainsId(itemId, count))
        {
            return true;
        }

        var amount = Game1.player.Items.ReduceId(itemId, count);
        if (amount == count)
        {
            return false;
        }

        var remaining = count - amount;
        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var container in containers)
        {
            amount = container.Items.ReduceId(itemId, remaining);
            remaining -= amount;
            if (remaining <= 0)
            {
                return false;
            }
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ShopMenu_HasTradeItem_postfix(ref bool __result, string itemId, int count)
    {
        if (__result)
        {
            return;
        }

        var amount = Game1.player.Items.CountId(itemId);
        var remaining = count - amount;
        if (remaining <= 0)
        {
            __result = true;
            return;
        }

        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var container in containers)
        {
            amount = container.Items.CountId(itemId);
            remaining -= amount;
            if (remaining > 0)
            {
                continue;
            }

            __result = true;
            return;
        }
    }
}