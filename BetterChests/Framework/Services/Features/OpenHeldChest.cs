namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Allows a chest to be opened while in the farmer's inventory.</summary>
internal sealed class OpenHeldChest : BaseFeature<OpenHeldChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;
    private readonly MenuHandler menuHandler;
    private readonly IPatchManager patchManager;
    private readonly ProxyChestFactory proxyChestFactory;

    /// <summary>Initializes a new instance of the <see cref="OpenHeldChest" /> class.</summary>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public OpenHeldChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        IManifest manifest,
        MenuHandler menuHandler,
        IModConfig modConfig,
        IPatchManager patchManager,
        ProxyChestFactory proxyChestFactory)
        : base(eventManager, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.patchManager = patchManager;
        this.proxyChestFactory = proxyChestFactory;

        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
                AccessTools.DeclaredMethod(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)),
                PatchType.Prefix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.OpenHeldChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);

        // Patches
        this.patchManager.Patch(this.UniqueId);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);

        // Patches
        this.patchManager.Unpatch(this.UniqueId);
    }

    // TODO: Recursive check

    /// <summary>Prevent adding chest into itself.</summary>
    [HarmonyPriority(Priority.High)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (__instance != item)
        {
            return true;
        }

        __result = item;
        return false;
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsActionButton()
            || !this.containerFactory.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var container)
            || container.OpenHeldChest != FeatureOption.Enabled)
        {
            return;
        }

        Log.Info("{0}: Opening held chest {1}", this.Id, container);
        this.inputHelper.Suppress(e.Button);
        container.Mutex?.RequestLock(
            () =>
            {
                container.ShowMenu(true);
            });
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (e.Container is FarmerContainer && (this.menuHandler.CurrentMenu as ItemGrabMenu)?.sourceItem == e.Item)
        {
            e.UnHighlight();
        }
    }

    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        if (this.proxyChestFactory.TryGetProxy(e.Item, out var chest)
            && (this.menuHandler.CurrentMenu as ItemGrabMenu)?.sourceItem == chest)
        {
            e.PreventTransfer();
        }
    }
}