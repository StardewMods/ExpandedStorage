namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
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
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly LocalizedTextManager localizedTextManager;
    private readonly IManifest manifest;
    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="commandHelper">Dependency used for handling console commands.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
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
        ItemGrabMenuManager itemGrabMenuManager,
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
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.localizedTextManager = localizedTextManager;
        this.manifest = manifest;
        this.patchManager = patchManager;
        this.configButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                assetHandler.Icons.Value,
                new Rectangle(0, 0, 16, 16),
                Game1.pixelZoom)
            {
                name = this.Id,
                hoverText = I18n.Button_Configure_Name(),
                myID = 42_069,
                region = ItemGrabMenu.region_organizationButtons,
            });

        // Commands
        commandHelper.Add("bc_player_config", "Configure the player backpack", this.ConfigurePlayer);

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
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

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
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

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

        // if (yOffset - (stepSize * (buttons.Count - 1)) < __instance.yPositionOnScreen)
        // {
        //     yOffset += ((stepSize * (buttons.Count - 1)) + __instance.yPositionOnScreen - yOffset) / 2;
        // }

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
            || this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container is null
            || !this.itemGrabMenuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.ShowMenu(this.itemGrabMenuManager.Top.Container);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!this.isActive.Value || !this.itemGrabMenuManager.CanFocus(this))
        {
            return;
        }

        if (!Context.IsPlayerFree
            || !this.Config.Controls.ConfigureChest.JustPressed()
            || !this.containerFactory.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var container)
            || container.Options.ConfigureChest != FeatureOption.Enabled)
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ConfigureChest);
        this.ShowMenu(container);
    }

    [Priority(1000)]
    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container?.Options.ConfigureChest != FeatureOption.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = true;
        this.itemGrabMenuManager.CurrentMenu.RepositionSideButtons();
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
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
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
        if (this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            this.itemGrabMenuManager.CurrentMenu.hoverText = this.configButton.Value.hoverText;
        }
    }

    private void ConfigurePlayer(string commands, string[] args)
    {
        if (!this.containerFactory.TryGetOne(Game1.player, out var container))
        {
            return;
        }

        this.ShowMenu(container);
    }

    private void ShowMenu(IStorageContainer container)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        this.Log.Info("{0}: Configuring {1}", this.Id, container);

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var defaultOptions = new DefaultStorageOptions();
        var options = new TemporaryStorageOptions(container.Options.GetActualOptions(), defaultOptions);
        var parentOptions = container.Options.GetParentOptions();
        this.genericModConfigMenuIntegration.Register(options.Reset, Save);

        gmcm.AddSectionTitle(this.manifest, () => container.DisplayName, container.ToString);

        gmcm.AddTextOption(
            this.manifest,
            () => options.StorageName,
            value => options.StorageName = value,
            I18n.Config_StorageName_Name,
            I18n.Config_StorageName_Tooltip);

        if (container.Options.StashToChest is not (RangeOption.Disabled or RangeOption.Default))
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
        if (container.Options.CategorizeChest is not (FeatureOption.Disabled or FeatureOption.Default))
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
        this.lastContainer.Value = container;
        return;

        void Save()
        {
            this.Log.Trace("Config changed: {0}\n{1}", container, options);
            options.Save();
        }
    }
}