namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>Allows a placed chest full of items to be picked up by the farmer.</summary>
internal sealed class CarryChest : BaseFeature<CarryChest>
{
    private static CarryChest instance = null!;

    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;
    private readonly IPatchManager patchManager;
    private readonly ProxyChestFactory proxyChestFactory;
    private readonly StatusEffectManager statusEffectManager;

    /// <summary>Initializes a new instance of the <see cref="CarryChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    /// <param name="statusEffectManager">Dependency used for adding and removing custom buffs.</param>
    public CarryChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IPatchManager patchManager,
        ProxyChestFactory proxyChestFactory,
        StatusEffectManager statusEffectManager)
        : base(eventManager, log, manifest, modConfig)
    {
        CarryChest.instance = this;
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.patchManager = patchManager;
        this.proxyChestFactory = proxyChestFactory;
        this.statusEffectManager = statusEffectManager;

        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
                AccessTools.DeclaredMethod(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)),
                PatchType.Postfix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CarryChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<OneSecondUpdateTickedEventArgs>(this.OnOneSecondUpdateTicked);

        // Patches
        this.patchManager.Patch(this.UniqueId);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<OneSecondUpdateTickedEventArgs>(this.OnOneSecondUpdateTicked);

        // Patches
        this.patchManager.Unpatch(this.UniqueId);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        GameLocation location,
        int x,
        int y,
        ref bool __result)
    {
        if (!__result
            || !CarryChest.instance.proxyChestFactory.TryGetProxy(__instance, out var chest)
            || !location.Objects.TryGetValue(
                new Vector2((int)(x / (float)Game1.tileSize), (int)(y / (float)Game1.tileSize)),
                out var obj)
            || obj is not Chest placedChest)
        {
            return;
        }

        // Copy data from chest
        placedChest.GlobalInventoryId = chest.GlobalInventoryId;
        placedChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
        foreach (var (key, value) in chest.modData.Pairs)
        {
            placedChest.modData[key] = value;
        }

        // Restore proxy
        CarryChest.instance.proxyChestFactory.TryRestoreProxy(placedChest);
    }

    private void OnOneSecondUpdateTicked(OneSecondUpdateTickedEventArgs e)
    {
        if (this.Config.CarryChestSlowLimit == 0)
        {
            return;
        }

        if (Game1.player.Items.Count(this.proxyChestFactory.IsProxy) >= this.Config.CarryChestSlowLimit)
        {
            this.statusEffectManager.AddEffect(StatusEffect.Overburdened);
            return;
        }

        if (this.statusEffectManager.HasEffect(StatusEffect.Overburdened))
        {
            this.statusEffectManager.RemoveEffect(StatusEffect.Overburdened);
        }
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || Game1.player.CurrentItem is Tool
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button)
            || (Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (!Game1.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out var obj) || obj is not Chest chest)
        {
            return;
        }

        // Allow swap behavior
        if (Game1.player.CurrentItem?.HasContextTag("swappable_chest") == true
            && chest.HasContextTag("swappable_chest")
            && Game1.player.CurrentItem.Name.Contains("Chest")
            && (Game1.player.CurrentItem.Name.Contains("Big") || !chest.ItemId.Contains("Big")))
        {
            return;
        }

        if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, e.Cursor.GrabTile, out var container)
            || container.Options.CarryChest != FeatureOption.Enabled)
        {
            return;
        }

        // Check carrying limits
        if (this.Config.CarryChestLimit > 0
            && Game1.player.Items.Count(this.proxyChestFactory.IsProxy) >= this.Config.CarryChestLimit)
        {
            Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
            this.inputHelper.Suppress(e.Button);
            return;
        }

        // Try to create proxy item
        if (!this.proxyChestFactory.TryCreateRequest(chest, out var request))
        {
            return;
        }

        // Try to add to inventory
        if (!Game1.player.addItemToInventoryBool(request.Item, true))
        {
            request.Cancel();
            return;
        }

        // Remove chest from world
        this.Log.Info(
            "{0}: Grabbed chest from {1} at ({2}, {3})",
            this.Id,
            Game1.player.currentLocation.Name,
            e.Cursor.GrabTile.X,
            e.Cursor.GrabTile.Y);

        request.Confirm();
        Game1.currentLocation.Objects.Remove(e.Cursor.GrabTile);
        Game1.playSound("pickUpItem");
        this.inputHelper.Suppress(e.Button);
    }
}