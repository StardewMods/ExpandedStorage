﻿namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class Configurator : Feature
{
    private const string Id = "furyx639.BetterChests/Configurator";

    private static readonly MethodBase ItemGrabMenuRepositionSideButtons = AccessTools.Method(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.RepositionSideButtons));

#nullable disable
    private static Configurator instance;
#nullable enable

    private readonly ModConfig config;
    private readonly PerScreen<ClickableTextureComponent> configButton;
    private readonly PerScreen<ItemGrabMenu?> currentMenu = new();
    private readonly PerScreen<StorageNode?> currentStorage = new();
    private readonly Harmony harmony;
    private readonly IModHelper helper;
    private readonly IManifest modManifest;

    private bool isActive;
    private EventHandler<StorageNode>? storageEdited;

    private Configurator(IModHelper helper, ModConfig config, IManifest manifest)
    {
        this.helper = helper;
        this.config = config;
        this.harmony = new(Configurator.Id);
        this.configButton = new(
            () => new(
                new(0, 0, Game1.tileSize, Game1.tileSize),
                helper.GameContent.Load<Texture2D>("furyx639.BetterChests/Icons"),
                new(0, 0, 16, 16),
                Game1.pixelZoom)
            {
                name = "Configure",
                hoverText = I18n.Button_Configure_Name(),
                myID = 42069,
            });

        this.modManifest = manifest;
    }

    /// <summary>Raised after an <see cref="StorageNode" /> has been edited.</summary>
    public static event EventHandler<StorageNode> StorageEdited
    {
        add => Configurator.instance.storageEdited += value;
        remove => Configurator.instance.storageEdited -= value;
    }

    private static ClickableTextureComponent ConfigButton => Configurator.instance.configButton.Value;

    private ItemGrabMenu? CurrentMenu
    {
        get => this.currentMenu.Value;
        set => this.currentMenu.Value = value;
    }

    private StorageNode? CurrentStorage
    {
        get => this.currentStorage.Value;
        set => this.currentStorage.Value = value;
    }

    /// <summary>Initializes <see cref="Configurator" />.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="Configurator" /> class.</returns>
    public static Feature Init(IModHelper helper, ModConfig config, IManifest manifest) =>
        Configurator.instance ??= new(helper, config, manifest);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

        // Patches
        this.harmony.Patch(
            Configurator.ItemGrabMenuRepositionSideButtons,
            postfix: new(typeof(Configurator), nameof(Configurator.ItemGrabMenu_RepositionSideButtons_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        this.helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Patches
        this.harmony.Unpatch(
            Configurator.ItemGrabMenuRepositionSideButtons,
            AccessTools.Method(typeof(Configurator), nameof(Configurator.ItemGrabMenu_RepositionSideButtons_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        if (__instance.allClickableComponents?.Contains(Configurator.ConfigButton) == false)
        {
            __instance.allClickableComponents.Add(Configurator.ConfigButton);
        }

        Configurator.ConfigButton.bounds.Y = 0;
        var buttons = new List<ClickableComponent>(
            new[]
            {
                __instance.organizeButton,
                __instance.fillStacksButton,
                __instance.colorPickerToggleButton,
                __instance.specialButton,
                Configurator.ConfigButton,
                __instance.junimoNoteIcon,
            }.Where(component => component is not null));

        var yOffset = buttons.Count switch
        {
            <= 3 => __instance.yPositionOnScreen + (__instance.height / 3),
            _ => __instance.ItemsToGrabMenu.yPositionOnScreen + __instance.ItemsToGrabMenu.height,
        };

        var stepSize = Game1.tileSize
            + buttons.Count switch
            {
                >= 4 => 8,
                _ => 16,
            };

        for (var index = 0; index < buttons.Count; ++index)
        {
            var button = buttons[index];
            if (index > 0 && buttons.Count > 1)
            {
                button.downNeighborID = buttons[index - 1].myID;
            }

            if (index < buttons.Count - 1 && buttons.Count > 1)
            {
                button.upNeighborID = buttons[index + 1].myID;
            }

            button.bounds.X = __instance.xPositionOnScreen + __instance.width;
            button.bounds.Y = yOffset - Game1.tileSize - (stepSize * index);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null || e.Button is not (SButton.MouseLeft or SButton.ControllerA))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!Configurator.ConfigButton.containsPoint(x, y) || BetterItemGrabMenu.Context is null)
        {
            return;
        }

        if (BetterItemGrabMenu.Context is
            {
                ConfigureMenu: InGameMenu.Categorize,
            })
        {
            Game1.activeClickableMenu = new ItemSelectionMenu(
                BetterItemGrabMenu.Context,
                BetterItemGrabMenu.Context.FilterMatcher,
                this.helper.Input,
                this.helper.Translation);
        }
        else
        {
            Config.SetupSpecificConfig(this.modManifest, BetterItemGrabMenu.Context, true);
            Integrations.GMCM.Api!.OpenModMenu(this.modManifest);
            this.isActive = true;
        }

        this.helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.config.ControlScheme.Configure.JustPressed() || Storages.CurrentItem is null)
        {
            return;
        }

        this.helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.Configure);
        Config.SetupSpecificConfig(this.modManifest, Storages.CurrentItem, true);
        Integrations.GMCM.Api!.OpenModMenu(this.modManifest);
        this.isActive = true;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is ItemGrabMenu
                {
                    shippingBin: false,
                } itemGrabMenu
                and not ItemSelectionMenu
            && BetterItemGrabMenu.Context is not null)
        {
            this.CurrentMenu = itemGrabMenu;
            this.CurrentStorage = BetterItemGrabMenu.Context;
            this.CurrentMenu.RepositionSideButtons();
            return;
        }

        this.CurrentMenu = null;
        if (!this.isActive || e.OldMenu?.GetType().Name != "SpecificModConfigMenu")
        {
            return;
        }

        this.isActive = false;
        Config.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            return;
        }

        if (this.CurrentStorage is
            {
                Data: Storage storageObject,
            })
        {
            this.storageEdited.InvokeAll(this, this.CurrentStorage);
            storageObject.ShowMenu();
            this.CurrentStorage = null;
            return;
        }

        Game1.activeClickableMenu = null;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        Configurator.ConfigButton.tryHover(x, y);
        e.SpriteBatch.Draw(
            Configurator.ConfigButton.texture,
            new(
                Configurator.ConfigButton.bounds.X + (8 * Game1.pixelZoom),
                Configurator.ConfigButton.bounds.Y + (8 * Game1.pixelZoom)),
            new(64, 0, 16, 16),
            Color.White,
            0f,
            new(8, 8),
            Configurator.ConfigButton.scale,
            SpriteEffects.None,
            0.86f);

        Configurator.ConfigButton.draw(e.SpriteBatch);
        if (Configurator.ConfigButton.containsPoint(x, y))
        {
            this.CurrentMenu.hoverText = Configurator.ConfigButton.hoverText;
        }
    }
}
