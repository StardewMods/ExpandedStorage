namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewValley.Menus;

/// <summary>Sort items in a chest using a customized criteria.</summary>
internal sealed class OrganizeChest : BaseFeature
{
    private static readonly MethodBase ItemGrabMenuOrganizeItemsInList = AccessTools.DeclaredMethod(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.organizeItemsInList));

    private readonly IModEvents events;

    private readonly Harmony harmony;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="OrganizeChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public OrganizeChest(IMonitor monitor, ModConfig config, IModEvents events, Harmony harmony, IInputHelper input)
        : base(monitor, nameof(OrganizeChest), () => config.OrganizeChest is not FeatureOption.Disabled)
    {
        this.events = events;
        this.harmony = harmony;
        this.input = input;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this.harmony.Patch(
            OrganizeChest.ItemGrabMenuOrganizeItemsInList,
            new(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Input.ButtonPressed -= this.OnButtonPressed;

        // Patches
        this.harmony.Unpatch(
            OrganizeChest.ItemGrabMenuOrganizeItemsInList,
            AccessTools.Method(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ItemGrabMenu_organizeItemsInList_prefix(ItemGrabMenu? __instance, IList<Item> items)
    {
        if (BetterItemGrabMenu.Context?.OrganizeChest != FeatureOption.Enabled)
        {
            return true;
        }

        __instance ??= Game1.activeClickableMenu as ItemGrabMenu;

        if (!object.Equals(__instance?.ItemsToGrabMenu.actualInventory, items))
        {
            return true;
        }

        var groupBy = BetterItemGrabMenu.Context.OrganizeChestGroupBy;
        var sortBy = BetterItemGrabMenu.Context.OrganizeChestSortBy;

        if (groupBy == GroupBy.Default && sortBy == SortBy.Default)
        {
            return true;
        }

        BetterItemGrabMenu.Context.OrganizeItems();
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        return false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseRight
            || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
            || BetterItemGrabMenu.Context is not
            {
                OrganizeChest: FeatureOption.Enabled,
            })
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.organizeButton?.containsPoint(x, y) != true)
        {
            return;
        }

        BetterItemGrabMenu.Context.OrganizeItems(true);
        this.input.Suppress(e.Button);
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        Game1.playSound("Ship");
    }
}
