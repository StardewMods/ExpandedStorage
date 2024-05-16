namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Handles changes to the active menu.</summary>
internal sealed class MenuHandler : GenericBaseService<MenuHandler>
{
    private static MenuHandler instance = null!;
    private readonly PerScreen<MenuManager> bottomMenu;

    private readonly Type? chestsAnywhereType;
    private readonly PerScreen<List<ICustomComponent>> components = new(() => []);
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<IClickableMenu?> currentMenu = new();
    private readonly IEventManager eventManager;
    private readonly PerScreen<ServiceLock?> focus = new();
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<MenuManager> topMenu;

    /// <summary>Initializes a new instance of the <see cref="MenuHandler" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to SMAPI events.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public MenuHandler(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents,
        IPatchManager patchManager)
        : base(log, manifest)
    {
        // Init
        MenuHandler.instance = this;
        this.containerFactory = containerFactory;
        this.eventManager = eventManager;
        this.inputHelper = inputHelper;

        this.topMenu = new PerScreen<MenuManager>(
            () => new MenuManager(eventManager, iconRegistry, inputHelper, log, manifest, this, modConfig));

        this.bottomMenu = new PerScreen<MenuManager>(
            () => new MenuManager(eventManager, iconRegistry, inputHelper, log, manifest, this, modConfig));

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        eventManager.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        eventManager.Subscribe<UpdateTickingEventArgs>(this.OnUpdateTicking);
        eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        eventManager.Subscribe<WindowResizedEventArgs>(this.OnWindowResized);
        modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;

        // Patches
        patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(IClickableMenu), nameof(IClickableMenu.SetChildMenu)),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.IClickableMenu_SetChildMenu_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)]),
                AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.InventoryMenu_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)]),
                AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.InventoryMenu_draw_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools
                    .GetDeclaredConstructors(typeof(ItemGrabMenu))
                    .Single(ctor => ctor.GetParameters().Length > 5),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.ItemGrabMenu_constructor_transpiler)),
                PatchType.Transpiler));

        this.chestsAnywhereType = Type.GetType(
            "Pathoschild.Stardew.ChestsAnywhere.Framework.Containers.ShippingBinContainer, ChestsAnywhere");

        if (this.chestsAnywhereType is not null)
        {
            var methodCanAcceptItem = AccessTools.DeclaredMethod(this.chestsAnywhereType, "CanAcceptItem");
            if (methodCanAcceptItem is not null)
            {
                patchManager.Add(
                    this.UniqueId,
                    new SavedPatch(
                        methodCanAcceptItem,
                        AccessTools.DeclaredMethod(
                            typeof(MenuHandler),
                            nameof(MenuHandler.ChestsAnywhere_CanAcceptType_Postfix)),
                        PatchType.Postfix));
            }
        }

        patchManager.Patch(this.UniqueId);
    }

    /// <summary>Gets the inventory menu manager for the bottom inventory menu.</summary>
    public MenuManager Bottom => this.bottomMenu.Value;

    /// <summary>Gets the inventory menu manager for the top inventory menu.</summary>
    public MenuManager Top => this.topMenu.Value;

    /// <summary>Gets the current menu.</summary>
    public IClickableMenu? CurrentMenu
    {
        get => this.currentMenu.Value;
        private set => this.currentMenu.Value = value;
    }

    /// <summary>Determines if the specified source object can receive focus.</summary>
    /// <param name="source">The object to check if it can receive focus.</param>
    /// <returns><c>true</c> if the source object can receive focus; otherwise, <c>false</c>.</returns>
    public bool CanFocus(object source) => this.focus.Value is null || this.focus.Value.Source == source;

    /// <summary>Tries to request focus for a specific object.</summary>
    /// <param name="source">The object that needs focus.</param>
    /// <param name="serviceLock">
    /// An optional output parameter representing the acquired service lock, or null if failed to
    /// acquire.
    /// </param>
    /// <returns><c>true</c> if focus was successfully acquired; otherwise, <c>false</c>.</returns>
    public bool TryGetFocus(object source, [NotNullWhen(true)] out IServiceLock? serviceLock)
    {
        serviceLock = null;
        if (this.focus.Value is not null && this.focus.Value.Source != source)
        {
            return false;
        }

        if (this.focus.Value is not null && this.focus.Value.Source == source)
        {
            serviceLock = this.focus.Value;
            return true;
        }

        this.focus.Value = new ServiceLock(source, this);
        serviceLock = this.focus.Value;
        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ChestsAnywhere_CanAcceptType_Postfix(Item item, ref bool __result)
    {
        if (!__result || MenuHandler.instance.Top.Container is null)
        {
            return;
        }

        __result = MenuHandler.instance.Bottom.HighlightMethod(item);
    }

    private static object? GetChestContext(Item? sourceItem, object? context) =>
        context switch
        {
            Chest chest => chest,
            SObject
            {
                heldObject.Value: Chest heldChest,
            } => heldChest,
            Building building when building.GetBuildingChest("Output") is
                { } outputChest => outputChest,
            GameLocation location when location.GetFridge() is
                { } fridge => fridge,
            _ => sourceItem,
        };

    private static int GetMenuCapacity(int capacity, object? context)
    {
        switch (context)
        {
            case Item item when MenuHandler.instance.containerFactory.TryGetOne(item, out var container):
            case Building building when MenuHandler.instance.containerFactory.TryGetOne(building, out container):
                return container.ResizeChest switch
                {
                    ChestMenuOption.Small => 9,
                    ChestMenuOption.Medium => 36,
                    ChestMenuOption.Large => 70,
                    _ when capacity is > 70 or -1 => 70,
                    _ => capacity,
                };

            default: return capacity > 70 ? 70 : capacity;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void IClickableMenu_SetChildMenu_postfix() => MenuHandler.instance.UpdateMenu();

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_postfix(InventoryMenu __instance, ref MenuManager? __state)
    {
        __state = __instance.Equals(MenuHandler.instance.Top.InventoryMenu)
            ? MenuHandler.instance.Top
            : __instance.Equals(MenuHandler.instance.Bottom.InventoryMenu)
                ? MenuHandler.instance.Bottom
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Restore original
        __instance.actualInventory = __state.Container.Items;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_prefix(InventoryMenu __instance, ref MenuManager? __state)
    {
        __state = __instance.Equals(MenuHandler.instance.Top.InventoryMenu)
            ? MenuHandler.instance.Top
            : __instance.Equals(MenuHandler.instance.Bottom.InventoryMenu)
                ? MenuHandler.instance.Bottom
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Apply operations
        var itemsDisplayingEventArgs = new ItemsDisplayingEventArgs(__state.Container);
        MenuHandler.instance.eventManager.Publish(itemsDisplayingEventArgs);
        __instance.actualInventory = itemsDisplayingEventArgs.Items.ToList();

        var defaultName = int.MaxValue.ToString(CultureInfo.InvariantCulture);
        var emptyIndex = -1;
        for (var index = 0; index < __instance.inventory.Count; ++index)
        {
            if (index >= __instance.actualInventory.Count)
            {
                __instance.inventory[index].name = defaultName;
                continue;
            }

            if (__instance.actualInventory[index] is null)
            {
                // Iterate to next empty index
                while (++emptyIndex < __state.Container.Items.Count
                    && __state.Container.Items[emptyIndex] is not null) { }

                if (emptyIndex >= __state.Container.Items.Count)
                {
                    __instance.inventory[index].name = defaultName;
                    continue;
                }

                __instance.inventory[index].name = emptyIndex.ToString(CultureInfo.InvariantCulture);
                continue;
            }

            var actualIndex = __state.Container.Items.IndexOf(__instance.actualInventory[index]);
            __instance.inventory[index].name =
                actualIndex > -1 ? actualIndex.ToString(CultureInfo.InvariantCulture) : defaultName;
        }
    }

    private static IEnumerable<CodeInstruction>
        ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Stloc_1))
            .Advance(-1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetChestContext))))
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetMenuCapacity))))
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetMenuCapacity))))
            .InstructionEnumeration();

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var cursorPos = e.Cursor.GetScaledScreenPixels().ToPoint();
        var baseMenu = Game1.activeClickableMenu?.GetChildMenu() as BaseMenu;
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                // Child menus
                if (baseMenu is not null)
                {
                    baseMenu.receiveLeftClick(cursorPos.X, cursorPos.Y);
                    this.inputHelper.Suppress(e.Button);
                    return;
                }

                // Components
                if (this
                    .components.Value.Where(c => c.Contains(e.Cursor.GetScaledScreenPixels()))
                    .Any(component => component.TryLeftClick(cursorPos.X, cursorPos.Y)))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;

            case SButton.MouseRight or SButton.ControllerB:
                // Child menus
                if (baseMenu is not null)
                {
                    baseMenu.receiveLeftClick(cursorPos.X, cursorPos.Y);
                    this.inputHelper.Suppress(e.Button);
                    return;
                }

                // Components
                if (this
                    .components.Value.Where(c => c.Contains(e.Cursor.GetScaledScreenPixels()))
                    .Any(component => component.TryRightClick(cursorPos.X, cursorPos.Y)))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;
        }
    }

    [Priority(int.MaxValue)]
    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        this.Top.Set(e.Parent, e.Top);
        this.Bottom.Set(e.Parent, e.Bottom);
    }

    [Priority(int.MinValue)]
    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        switch (this.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                // Draw overlay
                this.Top.Draw(e.SpriteBatch);
                this.Bottom.Draw(e.SpriteBatch);

                // Draw components
                foreach (var component in this.components.Value)
                {
                    component.Draw(e.SpriteBatch);
                }

                // Redraw foreground
                if (this.focus.Value is null)
                {
                    if (itemGrabMenu.hoverText != null
                        && (itemGrabMenu.hoveredItem == null || itemGrabMenu.ItemsToGrabMenu == null))
                    {
                        if (itemGrabMenu.hoverAmount > 0)
                        {
                            IClickableMenu.drawToolTip(
                                e.SpriteBatch,
                                itemGrabMenu.hoverText,
                                string.Empty,
                                null,
                                true,
                                -1,
                                0,
                                null,
                                -1,
                                null,
                                itemGrabMenu.hoverAmount);
                        }
                        else
                        {
                            IClickableMenu.drawHoverText(e.SpriteBatch, itemGrabMenu.hoverText, Game1.smallFont);
                        }
                    }

                    if (itemGrabMenu.hoveredItem != null)
                    {
                        IClickableMenu.drawToolTip(
                            e.SpriteBatch,
                            itemGrabMenu.hoveredItem.getDescription(),
                            itemGrabMenu.hoveredItem.DisplayName,
                            itemGrabMenu.hoveredItem,
                            itemGrabMenu.heldItem != null);
                    }
                    else if (itemGrabMenu.hoveredItem != null && itemGrabMenu.ItemsToGrabMenu != null)
                    {
                        IClickableMenu.drawToolTip(
                            e.SpriteBatch,
                            itemGrabMenu.ItemsToGrabMenu.descriptionText,
                            itemGrabMenu.ItemsToGrabMenu.descriptionTitle,
                            itemGrabMenu.hoveredItem,
                            itemGrabMenu.heldItem != null);
                    }

                    itemGrabMenu.heldItem?.drawInMenu(
                        e.SpriteBatch,
                        new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8),
                        1f);
                }

                break;

            case InventoryPage inventoryPage:
                // Draw overlay
                this.Top.Draw(e.SpriteBatch);
                this.Bottom.Draw(e.SpriteBatch);

                // Redraw foreground
                if (this.focus.Value is null)
                {
                    if (!string.IsNullOrEmpty(inventoryPage.hoverText))
                    {
                        if (inventoryPage.hoverAmount > 0)
                        {
                            IClickableMenu.drawToolTip(
                                e.SpriteBatch,
                                inventoryPage.hoverText,
                                inventoryPage.hoverTitle,
                                null,
                                true,
                                -1,
                                0,
                                null,
                                -1,
                                null,
                                inventoryPage.hoverAmount);
                        }
                        else
                        {
                            IClickableMenu.drawToolTip(
                                e.SpriteBatch,
                                inventoryPage.hoverText,
                                inventoryPage.hoverTitle,
                                inventoryPage.hoveredItem,
                                Game1.player.CursorSlotItem is not null);
                        }
                    }
                }

                break;

            case ShopMenu shopMenu:
                // Draw overlay
                this.Top.Draw(e.SpriteBatch);
                this.Bottom.Draw(e.SpriteBatch);

                // Redraw foreground
                if (this.focus.Value is null)
                {
                    if (shopMenu.hoveredItem?.IsRecipe ?? false)
                    {
                        shopMenu.heldItem?.drawInMenu(
                            e.SpriteBatch,
                            new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8),
                            1f,
                            1f,
                            0.9f,
                            StackDrawType.Draw,
                            Color.White,
                            true);
                    }
                    else
                    {
                        IClickableMenu.drawToolTip(
                            e.SpriteBatch,
                            shopMenu.hoverText,
                            shopMenu.boldTitleText,
                            shopMenu.hoveredItem as Item,
                            shopMenu.heldItem != null,
                            -1,
                            shopMenu.currency,
                            null,
                            -1,
                            null,
                            shopMenu.hoverPrice > 0 ? shopMenu.hoverPrice : -1);
                    }
                }

                break;

            default: return;
        }

        Game1.mouseCursorTransparency = 1f;
        Game1.activeClickableMenu.drawMouse(e.SpriteBatch);
    }

    [EventPriority((EventPriority)int.MaxValue)]
    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (Game1.options.showClearBackgrounds)
        {
            return;
        }

        switch (this.CurrentMenu)
        {
            case ItemGrabMenu
            {
                context: not null,
            }:
                // Redraw background
                e.SpriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                    Color.Black * 0.5f);

                break;

            case InventoryPage or ShopMenu: break;
            default: return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        foreach (var component in this.components.Value)
        {
            component.Update(mouseX, mouseY);
        }

        Game1.mouseCursorTransparency = 0f;
    }

    private void OnUpdateTicked(UpdateTickedEventArgs e) => this.UpdateMenuIfRequired();

    private void OnUpdateTicking(UpdateTickingEventArgs e) => this.UpdateMenuIfRequired();

    private void OnWindowResized(WindowResizedEventArgs e) => this.Top.Container?.ShowMenu();

    private void UpdateHighlightMethods()
    {
        switch (this.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                if (itemGrabMenu.ItemsToGrabMenu.highlightMethod?.Target != this.Top)
                {
                    var targetType = itemGrabMenu.ItemsToGrabMenu.highlightMethod?.Target?.GetType();
                    if (targetType is not null && targetType.Assembly == this.chestsAnywhereType?.Assembly)
                    {
                        return;
                    }

                    this.Top.OriginalHighlightMethod = itemGrabMenu.ItemsToGrabMenu.highlightMethod;
                    itemGrabMenu.ItemsToGrabMenu.highlightMethod = this.Top.HighlightMethod;
                }

                if (itemGrabMenu.inventory.highlightMethod?.Target != this.Bottom)
                {
                    var targetType = itemGrabMenu.inventory.highlightMethod?.Target?.GetType();
                    if (targetType is not null && targetType.Assembly == this.chestsAnywhereType?.Assembly)
                    {
                        return;
                    }

                    this.Bottom.OriginalHighlightMethod = itemGrabMenu.inventory.highlightMethod;
                    itemGrabMenu.inventory.highlightMethod = this.Bottom.HighlightMethod;
                }

                return;

            case InventoryPage inventoryPage when inventoryPage.inventory.highlightMethod?.Target != this.Bottom:
                this.Bottom.OriginalHighlightMethod = inventoryPage.inventory.highlightMethod;
                inventoryPage.inventory.highlightMethod = this.Bottom.HighlightMethod;
                return;

            case ShopMenu shopMenu when shopMenu.inventory.highlightMethod?.Target != this.Bottom:
                this.Bottom.OriginalHighlightMethod = shopMenu.inventory.highlightMethod;
                shopMenu.inventory.highlightMethod = this.Bottom.HighlightMethod;
                return;
        }
    }

    private void UpdateMenu()
    {
        var depth = -1;
        this.focus.Value = null;

        while (++depth != 2)
        {
            var newMenu = Game1.activeClickableMenu;
            IClickableMenu? parent = null;
            IClickableMenu? top = null;
            IClickableMenu? bottom = null;

            // Navigate down menus
            while (newMenu?.GetChildMenu() is not null)
            {
                newMenu = newMenu.GetChildMenu();
            }

            // Navigate up/sideways
            while (true)
            {
                switch (newMenu)
                {
                    case ItemGrabMenu menu:
                        parent = menu;
                        top = menu.showReceivingMenu ? menu.ItemsToGrabMenu : null;
                        bottom = menu.inventory;
                        break;
                    case GameMenu menu:
                        newMenu = menu.GetCurrentPage();
                        continue;
                    case InventoryPage menu:
                        parent = menu;
                        top = null;
                        bottom = menu.inventory;
                        break;
                    case ShopMenu menu:
                        parent = menu;
                        top = menu;
                        bottom = menu.inventory;
                        break;
                    case BaseMenu
                    {
                        Parent:
                        { } baseMenuParent,
                    }:
                        newMenu = baseMenuParent;
                        continue;
                    case not null when newMenu.GetParentMenu() is
                        { } parentMenu:
                        newMenu = parentMenu;
                        continue;
                }

                break;
            }

            if (this.CurrentMenu == newMenu)
            {
                return;
            }

            this.CurrentMenu = newMenu;
            if (this.CurrentMenu is null || parent is null)
            {
                this.Top.Container = null;
                this.Bottom.Container = null;
                this.components.Value.Clear();
                this.eventManager.Publish(new InventoryMenuChangedEventArgs(null, parent, top, bottom));
                return;
            }

            // Update top menu
            this.Top.Container = this.containerFactory.TryGetOne(top, out var topContainer) ? topContainer : null;
            if (topContainer is not null && this.CurrentMenu is ItemGrabMenu itemGrabMenu)
            {
                // Relaunch menu once
                if (depth == 0
                    && (itemGrabMenu.inventory.highlightMethod?.Target?.GetType() is null
                        || itemGrabMenu.inventory.highlightMethod?.Target?.GetType() != this.chestsAnywhereType))
                {
                    topContainer.ShowMenu();
                    continue;
                }

                itemGrabMenu.behaviorFunction = topContainer.GrabItemFromInventory;
                itemGrabMenu.behaviorOnItemGrab = topContainer.GrabItemFromChest;
            }

            // Update bottom menu
            this.bottomMenu.Value.Container = this.containerFactory.TryGetOne(bottom, out var bottomContainer)
                ? bottomContainer
                : null;

            // Reset filters
            this.UpdateHighlightMethods();
            this.components.Value.Clear();
            this.eventManager.Publish(new InventoryMenuChangedEventArgs(this.components.Value, parent, top, bottom));

            // Add components to the menu
            this.CurrentMenu.allClickableComponents ??= [];
            foreach (var component in this.components.Value)
            {
                this.CurrentMenu.allClickableComponents.Add(component.Component);
            }

            break;
        }
    }

    private void UpdateMenuIfRequired()
    {
        var menu = Game1.activeClickableMenu switch
        {
            { } menuWithChild when menuWithChild.GetChildMenu() is
                { } childMenu => childMenu,
            GameMenu gameMenu => gameMenu.GetCurrentPage(),
            _ => Game1.activeClickableMenu,
        };

        if (menu == this.CurrentMenu)
        {
            this.UpdateHighlightMethods();
            return;
        }

        this.UpdateMenu();
    }

    private sealed class ServiceLock(object source, MenuHandler menuHandler) : IServiceLock
    {
        public object Source => source;

        public void Release()
        {
            if (menuHandler.focus.Value == this)
            {
                menuHandler.focus.Value = null;
            }
        }
    }
}