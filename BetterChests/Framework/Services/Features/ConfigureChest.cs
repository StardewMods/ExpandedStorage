namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature<ConfigureChest>
{
    private static ConfigureChest instance = null!;

    private readonly PerScreen<ClickableTextureComponent> configButton;
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly LocalizedTextManager localizedTextManager;
    private readonly IManifest manifest;
    private readonly MenuHandler menuHandler;
    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="commandHelper">Dependency used for handling console commands.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="localizedTextManager">Dependency used for formatting and translating text.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ConfigureChest(
        AssetHandler assetHandler,
        ICommandHelper commandHelper,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        LocalizedTextManager localizedTextManager,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager)
        : base(eventManager, log, manifest, configManager)
    {
        ConfigureChest.instance = this;
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.localizedTextManager = localizedTextManager;
        this.manifest = manifest;
        this.patchManager = patchManager;
        this.configButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                assetHandler.UiTextures.Value,
                new Rectangle(0, 0, 16, 16),
                Game1.pixelZoom)
            {
                name = this.Id,
                hoverText = I18n.Button_Configure_Name(),
                myID = 42_069,
                region = ItemGrabMenu.region_organizationButtons,
            });

        // Commands
        commandHelper.Add("bc_player_config", I18n.Command_PlayerConfig(), this.ConfigurePlayer);
        commandHelper.Add("bc_reset_all", I18n.Command_ResetAll(), this.ResetAll);

        // Patches
        this.patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                AccessTools.DeclaredMethod(
                    typeof(ConfigureChest),
                    nameof(ConfigureChest.ItemGrabMenu_RepositionSideButtons_postfix)),
                PatchType.Postfix));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive =>
        this.Config.DefaultOptions.ConfigureChest != FeatureOption.Disabled
        && this.genericModConfigMenuIntegration.IsLoaded;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

        // Patches
        this.patchManager.Patch(this.UniqueId);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

        // Patches
        this.patchManager.Unpatch(this.UniqueId);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        if (!ConfigureChest.instance.isActive.Value)
        {
            return;
        }

        var configButton = ConfigureChest.instance.configButton.Value;
        if (__instance.allClickableComponents?.Contains(configButton) == false)
        {
            __instance.allClickableComponents.Add(configButton);
        }

        configButton.bounds.Y = 0;
        var buttons =
            new[]
                {
                    __instance.organizeButton,
                    __instance.fillStacksButton,
                    __instance.colorPickerToggleButton,
                    __instance.specialButton,
                    __instance.junimoNoteIcon,
                }
                .Where(component => component is not null)
                .ToList();

        buttons.Add(configButton);
        var stepSize = Game1.tileSize + buttons.Count switch { >= 4 => 8, _ => 16 };
        var yOffset = buttons[0].bounds.Y;

        var xPosition = Math.Max(buttons[0].bounds.X, __instance.okButton.bounds.X);

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

            button.bounds.X = xPosition;
            button.bounds.Y = yOffset - (stepSize * index);
        }

        foreach (var component in __instance.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right))
        {
            component.rightNeighborID =
                buttons.MinBy(c => Math.Abs(c.bounds.Center.Y - component.bounds.Center.Y))?.myID ?? -1;
        }
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || !this.menuHandler.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.lastContainer.Value = this.menuHandler.Top.Container ?? this.menuHandler.Bottom.Container;
        if (this.lastContainer.Value is null)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.ShowMenu();
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !this.Config.Controls.ConfigureChest.JustPressed()
            || (!this.containerFactory.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var container)
                && !this.containerFactory.TryGetOne(Game1.player.currentLocation, e.Cursor.Tile, out container)))
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ConfigureChest);
        this.lastContainer.Value = container;
        this.ShowMenu();
    }

    [Priority(1000)]
    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        switch (e.Parent)
        {
            case ItemGrabMenu itemGrabMenu:
                if (this.menuHandler.Top.Container?.ConfigureChest != FeatureOption.Enabled)
                {
                    this.isActive.Value = false;
                    return;
                }

                this.isActive.Value = true;
                itemGrabMenu.RepositionSideButtons();
                return;

            case InventoryPage inventoryPage:
                if (this.menuHandler.Bottom.Container?.ConfigureChest != FeatureOption.Enabled)
                {
                    this.isActive.Value = false;
                    return;
                }

                this.isActive.Value = true;
                inventoryPage.allClickableComponents.Add(this.configButton.Value);
                this.configButton.Value.bounds.X = inventoryPage.organizeButton.bounds.X;
                this.configButton.Value.bounds.Y = inventoryPage.organizeButton.bounds.Y
                    - Game1.tileSize
                    - (IClickableMenu.borderWidth / 2);

                return;

            case ShopMenu shopMenu:
                if (this.menuHandler.Top.Container?.ConfigureChest != FeatureOption.Enabled)
                {
                    this.isActive.Value = false;
                    return;
                }

                this.isActive.Value = true;
                this.configButton.Value.bounds.X =
                    shopMenu.upArrow.bounds.X + Game1.tileSize + (IClickableMenu.borderWidth / 2);

                this.configButton.Value.bounds.Y = shopMenu.upArrow.bounds.Y;
                return;

            default:
                this.isActive.Value = false;
                return;
        }
    }

    private void OnMenuChanged(MenuChangedEventArgs e)
    {
        if (this.lastContainer.Value is null
            || e.OldMenu?.GetType().Name != "SpecificModConfigMenu"
            || e.NewMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            return;
        }

        this.configManager.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            this.lastContainer.Value = null;
            return;
        }

        this.lastContainer.Value.ShowMenu();
        this.lastContainer.Value = null;
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.configButton.Value.tryHover(mouseX, mouseY);
        e.SpriteBatch.Draw(
            this.configButton.Value.texture,
            new Vector2(
                this.configButton.Value.bounds.X + (8 * Game1.pixelZoom),
                this.configButton.Value.bounds.Y + (8 * Game1.pixelZoom)),
            new Rectangle(64, 0, 16, 16),
            Color.White,
            0f,
            new Vector2(8, 8),
            this.configButton.Value.scale,
            SpriteEffects.None,
            0.86f);

        this.configButton.Value.draw(e.SpriteBatch);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        switch (this.menuHandler.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                itemGrabMenu.hoverText = this.configButton.Value.hoverText;
                return;

            case InventoryPage inventoryPage:
                inventoryPage.hoverText = this.configButton.Value.hoverText;
                return;

            case ShopMenu shopMenu:
                shopMenu.hoverText = this.configButton.Value.hoverText;
                return;
        }
    }

    private void ConfigurePlayer(string commands, string[] args)
    {
        if (!this.containerFactory.TryGetOne(Game1.player, out var container))
        {
            return;
        }

        this.lastContainer.Value = container;
        this.ShowMenu();
    }

    private void ResetAll(string commands, string[] args)
    {
        var defaultOptions = new DefaultStorageOptions();
        foreach (var container in this.containerFactory.GetAll())
        {
            defaultOptions.CopyTo(container);
        }
    }

    private void ShowMenu()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded || this.lastContainer.Value is null)
        {
            return;
        }

        this.Log.Info("{0}: Configuring {1}", this.Id, this.lastContainer.Value);

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var options = new DefaultStorageOptions();
        this.lastContainer.Value.ActualOptions.CopyTo(options);
        var parentOptions = this.lastContainer.Value.GetParentOptions();
        this.genericModConfigMenuIntegration.Register(() => new DefaultStorageOptions().CopyTo(options), Save);

        gmcm.AddSectionTitle(
            this.manifest,
            () => this.lastContainer.Value.DisplayName,
            this.lastContainer.Value.ToString);

        gmcm.AddTextOption(
            this.manifest,
            () => options.StorageName,
            value => options.StorageName = value,
            I18n.Config_StorageName_Name,
            I18n.Config_StorageName_Tooltip);

        gmcm.AddTextOption(
            this.manifest,
            () => options.StorageIcon,
            value => options.StorageIcon = value,
            I18n.Config_StorageIcon_Name,
            I18n.Config_StorageIcon_Tooltip);

        // Access Chest Priority
        if (this.lastContainer.Value.AccessChest is not RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => options.AccessChestPriority,
                value => options.AccessChestPriority = value,
                I18n.Config_AccessChestPriority_Name,
                I18n.Config_AccessChestPriority_Tooltip);
        }

        // Stash to Chest Priority
        if (this.lastContainer.Value.StashToChest is not RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => (int)options.StashToChestPriority,
                value => options.StashToChestPriority = (StashPriority)value,
                I18n.Config_StashToChestPriority_Name,
                I18n.Config_StashToChestPriority_Tooltip,
                -3,
                3,
                1,
                this.localizedTextManager.FormatStashPriority);
        }

        // Categorize Chest
        if (this.lastContainer.Value.CategorizeChest is not FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChestSearchTerm,
                value => options.CategorizeChestSearchTerm = value,
                I18n.Config_CategorizeChestSearchTerm_Name,
                I18n.Config_CategorizeChestSearchTerm_Tooltip);

            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChestIncludeStacks.ToStringFast(),
                value => options.CategorizeChestIncludeStacks = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CategorizeChestIncludeStacks_Name,
                I18n.Config_CategorizeChestIncludeStacks_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption(parentOptions?.CategorizeChestIncludeStacks));
        }

        gmcm.AddPageLink(this.manifest, "Main", I18n.Section_Main_Name, I18n.Section_Main_Description);
        this.configManager.AddMainOption("Main", I18n.Section_Main_Name, options, parentOptions: parentOptions);
        gmcm.OpenModMenu(this.manifest);
        return;

        void Save()
        {
            this.Log.Trace("Config changed: {0}\n{1}", this.lastContainer.Value, options);
            options.CopyTo(this.lastContainer.Value);

            if (this.lastContainer.Value is ChestContainer chestContainer)
            {
                chestContainer.Chest.fridge.Value = this.lastContainer.Value.CookFromChest is not RangeOption.Disabled;
            }
        }
    }
}