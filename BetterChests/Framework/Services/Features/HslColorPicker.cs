namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
using StardewValley.Objects;

// TODO: Draw farmer nearby using cursor distance

/// <summary>Adds a color picker that support hue, saturation, and lightness.</summary>
internal sealed class HslColorPicker : BaseFeature<HslColorPicker>
{
    private static HslColorPicker instance = null!;

    private readonly AssetHandler assetHandler;
    private readonly PerScreen<HslComponent?> colorPicker = new();
    private readonly IInputHelper inputHelper;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="HslColorPicker" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public HslColorPicker(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IPatchManager patchManager)
        : base(eventManager, log, manifest, modConfig)
    {
        HslColorPicker.instance = this;
        this.assetHandler = assetHandler;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.patchManager = patchManager;

        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw)),
                AccessTools.DeclaredMethod(
                    typeof(HslColorPicker),
                    nameof(HslColorPicker.DiscreteColorPicker_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(DiscreteColorPicker),
                    nameof(DiscreteColorPicker.getColorFromSelection)),
                AccessTools.DeclaredMethod(
                    typeof(HslColorPicker),
                    nameof(HslColorPicker.DiscreteColorPicker_getColorFromSelection_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(DiscreteColorPicker),
                    nameof(DiscreteColorPicker.getSelectionFromColor)),
                AccessTools.DeclaredMethod(
                    typeof(HslColorPicker),
                    nameof(HslColorPicker.DiscreteColorPicker_getSelectionFromColor_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.receiveLeftClick)),
                AccessTools.DeclaredMethod(
                    typeof(HslColorPicker),
                    nameof(HslColorPicker.DiscreteColorPicker_receiveLeftClick_prefix)),
                PatchType.Prefix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.HslColorPicker != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

        // Patches
        this.patchManager.Patch(this.UniqueId);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

        // Patches
        this.patchManager.Unpatch(this.UniqueId);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_draw_prefix(DiscreteColorPicker __instance)
    {
        if (HslColorPicker.instance.colorPicker.Value is not null)
        {
            __instance.visible = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getColorFromSelection_postfix(int selection, ref Color __result)
    {
        if (HslColorPicker.instance.colorPicker.Value is null)
        {
            return;
        }

        if (selection == 0)
        {
            __result = Color.Black;
            return;
        }

        var r = (byte)(selection & 0xFF);
        var g = (byte)((selection >> 8) & 0xFF);
        var b = (byte)((selection >> 16) & 0xFF);
        __result = new Color(r, g, b);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getSelectionFromColor_postfix(Color c, ref int __result)
    {
        if (HslColorPicker.instance.colorPicker.Value is null)
        {
            return;
        }

        if (c == Color.Black)
        {
            __result = 0;
            return;
        }

        __result = (c.R << 0) | (c.G << 8) | (c.B << 16);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_receiveLeftClick_prefix(DiscreteColorPicker __instance)
    {
        if (HslColorPicker.instance.colorPicker.Value is not null)
        {
            __instance.visible = false;
        }
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (this.colorPicker.Value is null
            || e.Button is not (SButton.MouseLeft or SButton.MouseRight or SButton.ControllerA or SButton.ControllerX))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.itemGrabMenuManager.CurrentMenu?.colorPickerToggleButton.containsPoint(mouseX, mouseY) == true)
        {
            this.inputHelper.Suppress(e.Button);
            Game1.playSound("drumkit6");
            Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
            return;
        }

        if (!Game1.player.showChestColorPicker)
        {
            return;
        }

        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA when this.colorPicker.Value.LeftClick(mouseX, mouseY):
            case SButton.MouseRight or SButton.ControllerX when this.colorPicker.Value.RightClick(mouseX, mouseY):
                this.inputHelper.Suppress(e.Button);
                Game1.playSound("coin");
                break;
        }
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not
            {
                itemToDrawColored: Chest,
            } chestColorPicker
            || this.itemGrabMenuManager.Top.Container is not ChestContainer container
            || container.Options.HslColorPicker != FeatureOption.Enabled)
        {
            this.colorPicker.Value = null;
            return;
        }

        this.colorPicker.Value = new HslComponent(
            this.assetHandler,
            chestColorPicker,
            this.inputHelper,
            this.itemGrabMenuManager,
            this.Config,
            () => container.Chest.playerChoiceColor.Value,
            c => container.Chest.playerChoiceColor.Value = c,
            this.itemGrabMenuManager.CurrentMenu.xPositionOnScreen
            - (2 * Game1.tileSize)
            - (IClickableMenu.borderWidth / 2),
            this.itemGrabMenuManager.CurrentMenu.yPositionOnScreen - 56 + (IClickableMenu.borderWidth / 2));
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (this.colorPicker.Value is null || !Game1.player.showChestColorPicker)
        {
            return;
        }

        this.colorPicker.Value.Draw(e.SpriteBatch);
    }
}