namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.</summary>
internal sealed class CollectItems : BaseFeature<CollectItems>
{
    private static CollectItems instance = null!;

    private readonly PerScreen<List<IStorageContainer>> cachedContainers = new(() => []);
    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;
    private readonly IPatchManager patchManager;
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="CollectItems" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public CollectItems(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        IModConfig modConfig,
        IPatchManager patchManager)
        : base(eventManager, modConfig)
    {
        CollectItems.instance = this;
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.patchManager = patchManager;

        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Debris), nameof(Debris.collect)),
                AccessTools.DeclaredMethod(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)),
                PatchType.Transpiler));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CollectItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<InventoryChangedEventArgs>(this.OnInventoryChanged);

        // Patches
        this.patchManager.Patch(this.UniqueId);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<InventoryChangedEventArgs>(this.OnInventoryChanged);

        // Patches
        this.patchManager.Unpatch(this.UniqueId);
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null)
        {
            return true;
        }

        // Redirect to vanilla if currently disabled
        if (Game1.player.modData.ContainsKey(CollectItems.instance.Prefix + "Disable"))
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        // Check if cache needs to be refreshed
        if (CollectItems.instance.resetCache.Value)
        {
            CollectItems.instance.RefreshEligible();
            CollectItems.instance.resetCache.Value = false;
        }

        // Redirect to vanilla if no storages are available
        if (!CollectItems.instance.cachedContainers.Value.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        // Attempt to add item to storages
        foreach (var storage in CollectItems.instance.cachedContainers.Value)
        {
            if (storage.TryAdd(item, out var remaining) && remaining is null)
            {
                return true;
            }
        }

        // Revert to vanilla if item could not be added to any storages
        return farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions) =>
        instructions.MethodReplacer(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
            AccessTools.Method(typeof(CollectItems), nameof(CollectItems.AddItemToInventoryBool)));

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        // Toggle Collect Items
        if (Context.IsPlayerFree && this.Config.Controls.ToggleCollectItems.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleCollectItems);
            var key = this.Prefix + "Disable";
            var disable = Game1.player.modData.ContainsKey(key);
            if (disable)
            {
                Game1.player.modData.Remove(key);
                Mod.Log.Info("{0}: Set collect items on", this.Id);
                return;
            }

            Game1.player.modData[key] = "true";
            Mod.Log.Info("{0}: Set collect items off", this.Id);
        }
    }

    private void OnInventoryChanged(InventoryChangedEventArgs e) => this.resetCache.Value = true;

    private void RefreshEligible()
    {
        this.cachedContainers.Value.Clear();
        foreach (var storage in this.containerFactory.GetAll(
            Game1.player,
            container => container.ChestFinder == FeatureOption.Enabled))
        {
            this.cachedContainers.Value.Add(storage);
        }
    }
}